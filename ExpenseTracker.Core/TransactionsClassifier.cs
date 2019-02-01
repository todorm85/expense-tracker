using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("ExpenseTracker.Tests")]
namespace ExpenseTracker.Core
{
    internal class TransactionsClassifier
    {
        public TransactionsClassifier(IEnumerable<Category> categories)
        {
            this.Categories = categories;
        }

        public IEnumerable<Category> Categories { get; set; }

        public void Classify(IEnumerable<Transaction> expenses)
        {
            foreach (var expense in expenses)
            {
                Category foundPhrase = this.Categories.FirstOrDefault(category => expense.Source != null && !string.IsNullOrWhiteSpace(category.ExpenseSourcePhrase) && expense.Source.Contains(category.ExpenseSourcePhrase));
                if (foundPhrase != null)
                {
                    expense.Category = foundPhrase.Name;
                }
            }
        }
    }
}