using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class TransactionsClassifier
    {
        public void Classify(IEnumerable<Transaction> expenses, IEnumerable<Category> categories)
        {
            foreach (var expense in expenses)
            {
                Category foundPhrase = categories.Where(category => expense.Details != null && !string.IsNullOrWhiteSpace(category.KeyWord) && expense.Details.Contains(category.KeyWord)).FirstOrDefault();
                if (foundPhrase != null)
                {
                    expense.Category = foundPhrase.Name;
                }
            }
        }
    }
}