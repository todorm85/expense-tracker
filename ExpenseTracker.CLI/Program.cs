using System;
using System.Text;
using ExpenseTracker.App;
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
            container.Dispose();

            mainMenu.Run();
        }

        private static IConfig GetConfig()
        {
            var user = Encoding.ASCII.GetString(Convert.FromBase64String(Environment.GetEnvironmentVariable("trckrm", EnvironmentVariableTarget.User)));
            var pass = Environment.GetEnvironmentVariable("trckr", EnvironmentVariableTarget.User);
            var config = new Config()
            {
                DbPath = Environment.GetEnvironmentVariable("trckrdb", EnvironmentVariableTarget.User),
                MailPass = pass,
                MailUser = user
            };

            return config;
        }
    }
}
