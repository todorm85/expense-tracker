using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Unity.Microsoft.DependencyInjection;

namespace ExpenseTracker.Web
{
    public class Program
    {
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .UseUnityServiceProvider()
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    ////webBuilder.ConfigureLogging(logging => logging.AddEventLog());
                    webBuilder.UseStartup<Startup>();
                });

        public static void Main(string[] args)
        {
            CreateHostBuilder(args).Build().Run();
        }
    }
}