using System;
using System.Text;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using ExpenseTracker.RestClient;
using ExpenseTracker.UI;
using Unity;
using Unity.Injection;

namespace ExpenseTracker.ConsoleClient
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var container = new UnityContainer();
            RegisterIoCTypes(container);
            container.Resolve<MenuBuilder>().Build<ExtendedMainMenu>().Run();
        }

        private static IUnityContainer RegisterIoCTypes(IUnityContainer container)
        {
            var renderer = new IOProvider();
            container.RegisterInstance<IOutputProvider>(renderer);
            container.RegisterInstance<IInputProvider>(renderer);

            RegisterServices(container);

            return container;
        }

        private static void RegisterServices(IUnityContainer container)
        {
            if (Settings.ClientMode == Settings.WebClientModeValue)
            {
                var authHeader = "Basic " + Convert.ToBase64String(Encoding.UTF8.GetBytes(Settings.WebClientAuth));
                container.RegisterType<IHttpClient, RestHttpClient>(new InjectionConstructor(Settings.WebServiceBase, authHeader));
                container.RegisterType<ITransactionsService, ExpensesRestClient>();
                container.RegisterType<IBudgetService, BudgetRestClient>();
                container.RegisterType<IBaseDataItemService<Category>, DataItemRestClient<Category>>(new InjectionConstructor(typeof(IHttpClient), "api/categories"));
            }
            else
            {
                container.RegisterType<IUnitOfWork, UnitOfWork>(new InjectionConstructor(Settings.DbPath));
                container.RegisterType<ITransactionsService, TransactionsService>();
                container.RegisterType<IBudgetService, BudgetService>();
                container.RegisterType<IBaseDataItemService<Category>, CategoriesService>();
                container.RegisterType<IBudgetCalculator, BudgetCalculator>();
            }
        }
    }
}