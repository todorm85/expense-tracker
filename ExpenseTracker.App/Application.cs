using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.Core.Budget;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Data;
using Unity;
using Unity.Injection;

namespace ExpenseTracker.App
{
    public class Application
    {
        public static void RegisterDependencies(IUnityContainer container, Config config)
        {
            container.RegisterInstance(config);

            container.RegisterInstance<IUnitOfWork>(new UnitOfWork(config.DbPath));
            container.RegisterType<IExpensesService, ExpensesService>();
            container.RegisterType<IBudgetService, BudgetService>();
            container.RegisterType(typeof(IRepository<>), typeof(BaseDataItemService<>));
            container.RegisterType<IBudgetCalculator, BudgetCalculator>();

            // allianz dependencies
            container.RegisterType<IExpenseMessageParser, AllianzExpenseMessageParser>("allianz");
            container.RegisterType<IExpenseMessageParser, RaiffeisenMessageParser>("raiffeisen");
            container.RegisterType<IMailClient, GmailClient>(new InjectionConstructor(config.MailUser, config.MailPass));
        }
    }
}