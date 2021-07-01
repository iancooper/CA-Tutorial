using System;
using GreetingsApp.Db;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpOverrides;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using MySqlConnector;
using Polly;

namespace GreetingsApp.Configuration
{
    public class Startup
    {
        public IConfiguration Configuration { get; private set; }
        
        public Startup(IWebHostEnvironment env)
        {
            BuildConfiguration(env);
        }

        private void BuildConfiguration(IWebHostEnvironment env)
        {
            var builder = new ConfigurationBuilder()
                .SetBasePath(env.ContentRootPath);

            builder.AddJsonFile("appsettings.json", optional: true, reloadOnChange: true);
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
        
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            app.UseForwardedHeaders(new ForwardedHeadersOptions
            {
                ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto
            });

            app.UseStaticFiles();
            app.UseRouting();

            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllers();
            });


            CheckDbIsUp(Configuration["Database:GreetingsDb"]);

            var context = app.ApplicationServices.GetService<DbContextOptions<GreetingContext>>();
            if (context == null) throw new Exception(string.Format("Could not find Db Context"));
            
            EnsureDatabaseCreated(context);

        }
        
        // This method gets called by the runtime. Use this method to add services to the container
        public void ConfigureServices(IServiceCollection services)
        {
            // Add framework services
            services.AddControllers();

            services.AddCors(options =>
            {
                options.AddPolicy("AllowAll", builder => builder.AllowAnyOrigin().AllowAnyMethod().AllowAnyHeader().AllowCredentials()
                );
            });

            var connectionString = Configuration["Database:Greetings"];
            if (string.IsNullOrEmpty(connectionString)) throw new Exception("Unable to read connection string for greetings db");
            
            var serverVersion = ServerVersion.AutoDetect(connectionString);

            services
                .AddDbContext<GreetingContext>(builder => builder
                    .UseMySql(connectionString, serverVersion)
                    .EnableSensitiveDataLogging()
                    .EnableDetailedErrors()
                );
        }
        
        private void EnsureDatabaseCreated(DbContextOptions<GreetingContext> dbGreetingsContext)
        {
            var dbContext = new GreetingContext(dbGreetingsContext);
            dbContext.Database.EnsureCreated();
        }
   }
}
