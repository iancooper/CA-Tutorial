using System;
using GreetingsCore.Adapters.Db;
using GreetingsCore.Adapters.Factories;
using GreetingsCore.Ports.Commands;
using GreetingsCore.Ports.Handlers;
using GreetingsCore.Ports.Mappers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Paramore.Brighter;
using Paramore.Brighter.MessagingGateway.RMQ;
using Paramore.Brighter.MessagingGateway.RMQ.MessagingGatewayConfiguration;
using Paramore.Brighter.ServiceActivator;
using Polly;
using Serilog;
using SimpleInjector;

namespace GreetingsWorker
{
 public class Program
    {
        public static void Main(string[] args)
        {

            var dispatcher = Configure();
            dispatcher.Receive();

            Console.WriteLine("Press Enter to stop ...");
            Console.ReadLine();

            dispatcher.End().Wait();
        }

        private static Dispatcher Configure()
        {
            var builder = new ConfigurationBuilder()
                .AddEnvironmentVariables();

            var configuration = builder.Build();

            Log.Logger = new LoggerConfiguration()
                .WriteTo.LiterateConsole()
                .CreateLogger();

            var container = new Container();

            DatabaseOptionsFactory(configuration, container);

            var dispatcher = RegisterDispatcher(container, configuration);
            
            EnsureDatabaseCreated(container);

            return dispatcher;
        }

        private static void DatabaseOptionsFactory(IConfigurationRoot configuration, Container container)
        {
            container.Register<DbContextOptions<GreetingContext>>( 
                () => new DbContextOptionsBuilder<GreetingContext>()
                    .UseMySql(configuration["Database:Greetings"])
                    .Options, 
                Lifestyle.Singleton);          
        }

        private static void EnsureDatabaseCreated(Container container)
        {
            var contextOptions = container.GetInstance<DbContextOptions<GreetingContext>>();
            using (var context = new GreetingContext(contextOptions))
            {
                context.Database.EnsureCreated();
            }
        }
        
        private static Dispatcher RegisterDispatcher(Container container, IConfigurationRoot configuration)
        {
            var handlerFactory = new HandlerFactory(container);
            var messageMapperFactory = new MessageMapperFactory(container);
            container.Register<IHandleRequests<RegreetCommand>, RegreetCommandHandler>();

            var subscriberRegistry = new SubscriberRegistry();
            subscriberRegistry.Register<RegreetCommand, RegreetCommandHandler>();

            //create policies
            var retryPolicy = Policy
                .Handle<Exception>()
                .WaitAndRetry(new[]
                {
                    TimeSpan.FromMilliseconds(50),
                    TimeSpan.FromMilliseconds(100),
                    TimeSpan.FromMilliseconds(150)
                });

            var circuitBreakerPolicy = Policy
                .Handle<Exception>()
                .CircuitBreaker(1, TimeSpan.FromMilliseconds(500));

            var policyRegistry = new PolicyRegistry
            {
                {CommandProcessor.RETRYPOLICY, retryPolicy},
                {CommandProcessor.CIRCUITBREAKER, circuitBreakerPolicy}
            };

            //create message mappers
            var messageMapperRegistry = new MessageMapperRegistry(messageMapperFactory)
            {
                {typeof(RegreetCommand), typeof(RegreetCommandMessageMapper)}
            };

            var amqpUri = configuration["BROKER"];
            //create the gateway
            var rmqConnnection = new RmqMessagingGatewayConnection
            {
                AmpqUri = new AmqpUriSpecification(new Uri(amqpUri)),
                Exchange = new Exchange("paramore.brighter.exchange"),
            };

            var rmqMessageConsumerFactory = new RmqMessageConsumerFactory(rmqConnnection);

            var dispatcher = DispatchBuilder.With()
                .CommandProcessor(CommandProcessorBuilder.With()
                    .Handlers(new HandlerConfiguration(subscriberRegistry, handlerFactory))
                    .Policies(policyRegistry)
                    .NoTaskQueues()
                    .RequestContextFactory(new InMemoryRequestContextFactory())
                    .Build())
                .MessageMappers(messageMapperRegistry)
                .DefaultChannelFactory(new InputChannelFactory(rmqMessageConsumerFactory))
                .Connections(new Connection[]
                {
                    new Connection<RegreetCommand>(
                        new ConnectionName("paramore.example.greeting.regreet"),
                        new ChannelName("greeting.regreet.command"),
                        new RoutingKey("greeting.regreet.command"),
                        timeoutInMilliseconds: 200)
                }).Build();

            return dispatcher;
        }
    }
}