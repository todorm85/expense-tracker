using System;
using System.Diagnostics;
using System.IO;
using System.Text;
using ExpenseTracker.App;
using Microsoft.Extensions.Configuration;
using Unity;

namespace ExpenseTracker.CoreCLI
{
    class Program
    {
        static void Main()
        {
            var container = new UnityContainer();
            Application.RegisterDependencies(container, GetConfig());
            var mainMenu = container.Resolve<MainMenu>();
            mainMenu.Run();
            container.Dispose();
        }

        private static Config GetConfig()
        {
            IConfiguration config = new ConfigurationBuilder()
                .SetBasePath(GetBasePath())
                .AddJsonFile(".\\settings.json", optional: true, reloadOnChange: true)
                .Build();

            var appConfig = config.Get<Config>();

            return appConfig ?? new Config();
        }

        // https://stackoverflow.com/questions/58307558/how-can-i-get-my-net-core-3-single-file-app-to-find-the-appsettings-json-file
        private static string GetBasePath()
        {
            using var processModule = Process.GetCurrentProcess().MainModule;
            return Path.GetDirectoryName(processModule?.FileName);
        }
    }
}
