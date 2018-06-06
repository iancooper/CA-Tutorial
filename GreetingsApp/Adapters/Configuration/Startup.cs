using System;
using System.Collections.Immutable;
using GreetingsCore.Adapters.BrighterFactories;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.DI;
using GreetingsCore.Adapters.Factories;
using GreetingsCore.Ports;
using GreetingsCore.Ports.Commands;
using GreetingsCore.Ports.Handlers;
using GreetingsCore.Ports.Mappers;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.AspNetCore.Mvc.ViewComponents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using MySql.Data.MySqlClient;
using Paramore.Brighter;
using Paramore.Brighter.MessageStore.MySql;
using Paramore.Brighter.MessagingGateway.RMQ;
using Paramore.Brighter.MessagingGateway.RMQ.MessagingGatewayConfiguration;
using Paramore.Darker;
using Paramore.Darker.AspNetCore;
using Paramore.Darker.Builder;
using Paramore.Darker.Policies;
using Paramore.Darker.QueryLogging;
using Paramore.Darker.SimpleInjector;
using Polly;
using SimpleInjector;
using SimpleInjector.Integration.AspNetCore.Mvc;
using SimpleInjector.Lifestyles;
using PolicyRegistry = Paramore.Brighter.PolicyRegistry;

namespace GreetingsApp.Adapters.Configuration
{
    public class Startup
    {
        private readonly Container _container;

        public IConfiguration Configuration { get; private set; }
        
        public Startup(IHostingEnvironment env)
        {
            //use a sensible constructor resolution approach
            _container = new Container();
            _container.Options.ConstructorResolutionBehavior = new MostResolvableParametersConstructorResolutionBehavior(_container);

            BuildConfiguration(env);
        }

        private void BuildConfiguration(IHostingEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);

            if (env.IsEnvironment("Development"))
            {
                // This will push telemetry data through Application Insights pipeline faster, allowing you to view results immediately.
                builder.AddApplicationInsightsSettings(developerMode: true);
            }

            builder.AddEnvironmentVariables();
            Configuration = builder.Build();
        }
        
        private void CheckDbIsUp(string connectionString)
        {
            var policy = Policy.Handle<MySqlException>().WaitAndRetryForever(
                retryAttempt => TimeSpan.FromSeconds(2),
                (exception, timespan) =>
                {
                    Console.WriteLine($"Healthcheck: Waiting for the database {connectionString} to come online - {exception.Message}");
                });

            policy.Execute(() =>
            {
                using (var conn = new MySqlConnection(connectionString))
                {
                    conn.Open();
                }
            });
        }
        
        private static void CreateMessageTable(string dataSourceTestDb, string tableNameMessages)
        {
            try
            {
                using (var sqlConnection = new MySqlConnection(dataSourceTestDb))
                {
                    sqlConnection.Open();
                    using (var command = sqlConnection.CreateCommand())
                    {
                        command.CommandText = MySqlMessageStoreBuilder.GetDDL(tableNameMessages);
                        command.ExecuteScalar();
                    }
                }
                
            }
            catch (System.Exception e)
            {
                Console.WriteLine($"Issue with creating MessageStore table, {e.Message}");
            }
        }
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IHostingEnvironment env, ILoggerFactory loggerFactory, IApplicationLifetime appLifetime)
        {
            InitializeContainer(app);
            _container.Verify();

            loggerFactory.AddConsole(Configuration.GetSection("Logging"));
            loggerFactory.AddDebug();

            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseCors("AllowAll");

            app.UseMvc();

            CheckDbIsUp(Configuration["Database:GreetingsDb"]);

            EnsureDatabaseCreated();
            
            CreateMessageTable(Configuration["Database:MessageStore"], Configuration["Database:MessageTableName"]);

        }
        
        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services.
            services.AddApplicationInsightsTelemetry(Configuration);
            
            services.AddMvc();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials()
                );
            });

            IntegrateSimpleInjector(services);
        }
        
        private void EnsureDatabaseCreated()
        {
            var contextOptions = _container.GetInstance<DbContextOptions<GreetingContext>>();
            using (var context = new GreetingContext(contextOptions))
            {
                context.Database.EnsureCreated();
            }
        }

        private void IntegrateSimpleInjector(IServiceCollection services)
        {
            _container.Options.DefaultScopedLifestyle = new AsyncScopedLifestyle();

            services.AddSingleton<IHttpContextAccessor, HttpContextAccessor>();

            services.AddSingleton<IControllerActivator>(
                new SimpleInjectorControllerActivator(_container));
            services.AddSingleton<IViewComponentActivator>(
                new SimpleInjectorViewComponentActivator(_container));

            services.EnableSimpleInjectorCrossWiring(_container);
            services.UseSimpleInjectorAspNetRequestScoping(_container);
   
        }
       
        private void InitializeContainer(IApplicationBuilder app)
        {
            _container.Register<DbContextOptions<GreetingContext>>( 
                () => new DbContextOptionsBuilder<GreetingContext>()
                    .UseMySql(Configuration["Database:Greetings"])
                    .Options, 
                Lifestyle.Singleton);
            
            // Add application presentation components:
            _container.RegisterMvcControllers(app);
            _container.RegisterMvcViewComponents(app);

            RegisterCommandProcessor();
            RegisterQueryProcessor();

            // Cross-wire ASP.NET services (if any). For instance:
            _container.CrossWire<ILoggerFactory>(app);
            // NOTE: Prevent cross-wired instances as much as possible.
            // See: https://simpleinjector.org/blog/2016/07/
        }

        private void RegisterQueryProcessor()
        {
            
            var retryPolicy = Policy.Handle<Exception>().WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(150) });
            var circuitBreakerPolicy = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromMilliseconds(500));
            var policyRegistry = new Paramore.Darker.Policies.PolicyRegistry
            {
                {Paramore.Darker.Policies.Constants.RetryPolicyName, retryPolicy}, 
                {Paramore.Darker.Policies.Constants.CircuitBreakerPolicyName, circuitBreakerPolicy}
            };
            
            var queryProcessor = QueryProcessorBuilder.With()
                .SimpleInjectorHandlers(_container, opts => 
                    opts.WithQueriesAndHandlersFromAssembly(typeof(GreetingsAllQuery).Assembly))
                .InMemoryQueryContextFactory()
                .Policies(policyRegistry)
                .JsonQueryLogging()
                .Build();

                _container.Register<IQueryProcessor>(() => queryProcessor, Lifestyle.Singleton);
        }

        private void RegisterCommandProcessor()
        {
            //create handler 
            var servicesHandlerFactory = new ServicesHandlerFactoryAsync(_container);
            var subscriberRegistry = new SubscriberRegistry();
            _container.Register<IHandleRequestsAsync<AddGreetingCommand>, AddGreetingCommandHandlerAsync>(Lifestyle.Scoped);

            subscriberRegistry.RegisterAsync<AddGreetingCommand, AddGreetingCommandHandlerAsync>();

            //create policies
            var retryPolicy = Policy.Handle<Exception>().WaitAndRetry(new[] { TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(150) });
            var circuitBreakerPolicy = Policy.Handle<Exception>().CircuitBreaker(1, TimeSpan.FromMilliseconds(500));
            var retryPolicyAsync = Policy.Handle<Exception>().WaitAndRetryAsync(new[] { TimeSpan.FromMilliseconds(50), TimeSpan.FromMilliseconds(100), TimeSpan.FromMilliseconds(150) });
            var circuitBreakerPolicyAsync = Policy.Handle<Exception>().CircuitBreakerAsync(1, TimeSpan.FromMilliseconds(500));
            var policyRegistry = new PolicyRegistry()
            {
                { CommandProcessor.RETRYPOLICY, retryPolicy },
                { CommandProcessor.CIRCUITBREAKER, circuitBreakerPolicy },
                { CommandProcessor.RETRYPOLICYASYNC, retryPolicyAsync },
                { CommandProcessor.CIRCUITBREAKERASYNC, circuitBreakerPolicyAsync }
            };

            var messagingGatewayConfiguration = RmqGatewayBuilder.With.Uri(new Uri(Configuration["BROKER"]))
                .Exchange("paramore.brighter.exchange")
                .DefaultQueues();

            var gateway = new RmqMessageProducer(messagingGatewayConfiguration);
            var sqlMessageStore = new MySqlMessageStore(
                new MySqlMessageStoreConfiguration(Configuration["Database:MessageStore"], 
                    Configuration["Database:MessageTableName"])
                );

             var messageMapperFactory = new MessageMapperFactory(_container);
            _container.Register<IAmAMessageMapper<RegreetCommand>, RegreetCommandMessageMapper>();

            var messageMapperRegistry = new MessageMapperRegistry(messageMapperFactory)
            {
                {typeof(RegreetCommand), typeof(RegreetCommandMessageMapper)},
            };

            var messagingConfiguration = new MessagingConfiguration(
                messageStore: sqlMessageStore,
                messageProducer: gateway,
                messageMapperRegistry: messageMapperRegistry);

            var commandProcessor = CommandProcessorBuilder.With()
                .Handlers(new Paramore.Brighter.HandlerConfiguration(subscriberRegistry, servicesHandlerFactory))
                .Policies(policyRegistry)
                .TaskQueues(messagingConfiguration)
                .RequestContextFactory(new Paramore.Brighter.InMemoryRequestContextFactory())
                .Build();

            _container.RegisterSingleton<IAmACommandProcessor>(commandProcessor);
        }



    }
    
}
