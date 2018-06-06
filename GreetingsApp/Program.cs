using System.IO;
using GreetingsApp.Adapters.Configuration;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Logging;

namespace GreetingsApp
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var host = new WebHostBuilder()
                .UseKestrel()
                .UseUrls("http://*:5000")  // listen on port 5000 on all network interfaces; needed for containers
                .UseContentRoot(Directory.GetCurrentDirectory())
                .CaptureStartupErrors(true)
                .UseSetting("detailedErrors", "true")
                .ConfigureLogging((hostingContext, logging) =>
                {
                    logging.AddConsole();
                    logging.AddDebug();
                })
                .UseStartup<Startup>()
                .Build();

            host.Run();
         }

   }
}
