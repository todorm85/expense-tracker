using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using ExpenseTracker.Core;

[assembly: InternalsVisibleTo("ExpenseTracker.Allianz.Tests")]
namespace ExpenseTracker.Allianz
{
    public class TransactionsClassifier : ITransactionsClassifier
    {
        private readonly IGenericRepository<Category> categories;

        public TransactionsClassifier(IUnitOfWork uow)
        {
            this.categories = uow.GetDataItemsRepo<Category>();
        }

        public void Classify(IEnumerable<Transaction> expenses)
        {
            foreach (var expense in expenses)
            {
                Category foundPhrase = this.categories.GetAll(category => expense.Details != null && !string.IsNullOrWhiteSpace(category.ExpenseSourcePhrase) && expense.Details.Contains(category.ExpenseSourcePhrase)).FirstOrDefault();
                if (foundPhrase != null)
                {
                    expense.Category = foundPhrase.Name;
                }
            }
        }
    }
}