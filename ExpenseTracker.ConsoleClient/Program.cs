using ExpenseTracker.Core;
using ExpenseTracker.Data;
using ExpenseTracker.RestClient;
using ExpenseTracker.UI;
using Microsoft.Practices.Unity;
using System;
using System.Text;

namespace ExpenseTracker.ConsoleClient
{
    internal class Program
    {
        internal static bool isWebClientMode;
        private static string webServiceBase;
        private static string auth;

        public static void Main(string[] args)
        {
            ProcessArguments(args);
            var container = RegisterIoCTypes();
            container.Resolve<ExtendedMenuBuilder>().Build().Run();
        }

        private static void ProcessArguments(string[] args)
        {
            for (int i = 0; i < args.Length; i++)
            {
                if (args[i] == "web")
                {
                    isWebClientMode = true;
                    webServiceBase = args[i+1];
                    new Renderer().WriteLine($"Web client mode enabled. Base address = {webServiceBase}");
                }

                if (args[i] == "auth")
                {
                    auth = args[i + 1];
                }
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
                var authHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(auth));
                container.RegisterType<IHttpClient, RestHttpClient>(new InjectionConstructor(webServiceBase, authHeader));
                container.RegisterType<IExpensesService, ExpensesRestClient>();
                container.RegisterType<IBudgetService, BudgetRestClient>();
                container.RegisterType<IBaseDataItemService<Category>, DataItemRestClient<Category>>(new InjectionConstructor(typeof(IHttpClient), "api/categories"));
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