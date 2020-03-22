using System;
using System.Text;
using ExpenseTracker.App;
using ExpenseTracker.ConsoleClient;
using ExpenseTracker.UI;
using Unity;

namespace ExpenseTracker.CoreCLI
{
    class Program
    {
        static void Main()
        {
            var container = new UnityContainer();
            Application.RegisterDependencies(container);
            var mainMenu = container.Resolve<MainMenu>();
            container.Dispose();

            mainMenu.Run();
        }
    }
}
