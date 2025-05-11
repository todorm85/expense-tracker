using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.Core.Budget;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Data;
using ExpenseTracker.Integrations.ApiClients.Trading212;
using ExpenseTracker.Integrations.Email.MessageParsers;
using ExpenseTracker.Integrations.Files;
using Microsoft.Extensions.DependencyInjection;

namespace ExpenseTracker.App
{
    public class Application
    {
        public static void RegisterDependencies(IServiceCollection services, Config config)
        {
            services.AddSingleton(config);

            services.AddSingleton<IUnitOfWork>(new UnitOfWork(config.DbPath));
            services.AddScoped<IExpensesService, ExpensesService>();
            services.AddScoped<IBudgetService, BudgetService>();
            services.AddScoped(typeof(IRepository<>), typeof(BaseDataItemService<>));
            services.AddScoped<IBudgetCalculator, BudgetCalculator>();

            // Register named dependencies using factories
            services.AddSingleton<IExpenseMessageParser, AllianzExpenseMessageParser>();
            services.AddSingleton<IExpenseMessageParser, RaiffeisenMessageParser>();
            services.AddSingleton<IExpenseMessageParser, FibankMessageParser>();

            // Register IMailClient with constructor parameters
            services.AddSingleton<IMailClient>(sp => new GmailClient(config.MailUser, config.MailPass));

            services.AddScoped<MailImporter>();
            services.AddScoped<Trading212Importer>();
            services.AddSingleton<Trading212CsvParser>();
            services.AddSingleton<RaiffeizenXmlFileParser>();
            services.AddSingleton<RevolutCsvParser>();
            services.AddSingleton<AllianzCsvParser>();
        }
    }
}