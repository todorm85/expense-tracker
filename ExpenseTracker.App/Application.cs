using System;
using System.Text;
using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using Unity;
using Unity.Injection;

namespace ExpenseTracker.App
{
    public class Application
    {
        public static void RegisterDependencies(IUnityContainer container, IConfig config)
        {
            container.RegisterInstance<IConfig>(config);

            container.RegisterType<IUnitOfWork, UnitOfWork>(new InjectionConstructor(config.DbPath));
            container.RegisterType<ITransactionsService, TransactionsService>();
            container.RegisterType<IBudgetService, BudgetService>();
            container.RegisterType<IBaseDataItemService<Category>, CategoriesService>();
            container.RegisterType<IBudgetCalculator, BudgetCalculator>();

            // allianz dependencies
            container.RegisterType<ITransactionBuilder, TransactionBuilder>();
            container.RegisterType<IExpenseMessageParser, AllianzExpenseMessageParser>("allianz");
            container.RegisterType<IExpenseMessageParser, RaiffeisenMessageParser>("raiffeisen");
            container.RegisterType<IMailClient, GmailClient>(new InjectionConstructor(config.MailUser, config.MailPass));
        }
    }
}
