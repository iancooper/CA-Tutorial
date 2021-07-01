using System.IO;
using GreetingsApp.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace GreetingsApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }

        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    webBuilder.UseKestrel();
                    webBuilder.UseUrls("http://*:5000"); // listen on port 5000 on all network interfaces; needed for containers
                    webBuilder.UseContentRoot(Directory.GetCurrentDirectory());
                    webBuilder.CaptureStartupErrors(true);
                    webBuilder.UseSetting("detailedErrors", "true");
                    webBuilder.ConfigureLogging((hostingContext, logging) =>
                    {
                        logging.AddConsole();
                        logging.AddDebug();
                    });
                    webBuilder.UseStartup<Startup>();
                });
    }
}
