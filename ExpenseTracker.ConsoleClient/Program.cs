using ExpenseTracker.Core;
using ExpenseTracker.Data;
using ExpenseTracker.RestClient;
using ExpenseTracker.UI;
using Microsoft.Practices.Unity;

namespace ExpenseTracker.ConsoleClient
{
    internal class Program
    {
        internal static bool isWebClientMode;
        private static string webServiceBase;

        public static void Main(string[] args)
        {
            ProcessArguments(args);
            var container = RegisterIoCTypes();
            container.Resolve<ExtendedMenuBuilder>().Build().Run();
        }

        private static void ProcessArguments(string[] args)
        {
            if (args[0] == "web")
            {
                isWebClientMode = true;
                webServiceBase = args[1];
                new Renderer().WriteLine($"Web client mode enabled. Base address = {webServiceBase}");
            }
        }

        private static IUnityContainer RegisterIoCTypes()
        {
            var container = new UnityContainer();
            container.RegisterInstance<IOutputRenderer>(new Renderer());
            container.RegisterType<IMenuFactory, MenuFactory>();

            RegisterServices(container);

            return container;
        }

        private static void RegisterServices(UnityContainer container)
        {
            if (isWebClientMode)
            {
                container.RegisterType<IExpensesService, ExpensesRestClient>(new InjectionConstructor(webServiceBase));
                container.RegisterType<IBudgetService, BudgetService>();
                container.RegisterType<IBaseDataItemService<Category>, CategoriesService>();
            }
            else
            {
                container.RegisterType<IUnitOfWork, UnitOfWork>(new InjectionConstructor(Utils.GetDbPath()));
                container.RegisterType<IExpensesService, ExpensesService>();
                container.RegisterType<IBudgetService, BudgetService>();
                container.RegisterType<IBaseDataItemService<Category>, CategoriesService>();
            }
        }
    }
}