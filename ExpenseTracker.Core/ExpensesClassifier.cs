using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    internal class ExpensesClassifier
    {
        public ExpensesClassifier(IEnumerable<Category> categories)
        {
            this.Categories = categories;
        }

        public IEnumerable<Category> Categories { get; set; }

        public void Classify(IEnumerable<Expense> expenses)
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