using ExpenseTracker.Allianz;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using ExpenseTracker.UI;
using Unity;
using Unity.Injection;

namespace ExpenseTracker.App
{
    public class Application
    {
        internal static string DbPath;

        public Application(string dbPath, IOutputProvider output, IInputProvider input)
        {
            DbPath = dbPath;
            Runtime.Output = output;
            Runtime.Input = input;

            var container = new UnityContainer();
            this.RegisterDependencies(container);
            container.Resolve<MainMenu>().Run();
            container.Dispose();
        }

        private void RegisterDependencies(IUnityContainer container)
        {
            container.RegisterType<IUnitOfWork, UnitOfWork>(new InjectionConstructor(DbPath));
            container.RegisterType<ITransactionsService, TransactionsService>();
            container.RegisterType<IBudgetService, BudgetService>();
            container.RegisterType<IBaseDataItemService<Category>, CategoriesService>();
            container.RegisterType<IBudgetCalculator, BudgetCalculator>();

            // allianz dependencies
            container.RegisterType<ITransactionBuilder, TransactionBuilder>();
            container.RegisterType<IExpenseMessageParser, ExpenseMessageParser>();
        }
    }
}
