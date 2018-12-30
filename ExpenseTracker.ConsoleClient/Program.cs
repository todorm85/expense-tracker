using ExpenseTracker.Core;
using ExpenseTracker.Data;
using ExpenseTracker.UI;
using Microsoft.Practices.Unity;

namespace ExpenseTracker.ConsoleClient
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var container = RegisterIoCTypes();
            container.Resolve<MenuBuilder>().Build().Run();
        }

        private static IUnityContainer RegisterIoCTypes()
        {
            var container = new UnityContainer();
            var config = new Config()
            {
                DbPath = Utils.GetDbPath()
            };

            container.RegisterInstance(config);
            container.RegisterInstance<IOutputRenderer>(new Renderer());

            container.RegisterType<IUnitOfWork, UnitOfWork>(new InjectionConstructor(Utils.GetDbPath()));

            container.RegisterType<IExpensesService, ExpensesService>();
            container.RegisterType<IBudgetService, BudgetService>();
            container.RegisterType<IBaseDataItemService<Category>, CategoriesService>();

            container.RegisterType<IMenuFactory, MenuFactory>();
            return container;
        }
    }
}
