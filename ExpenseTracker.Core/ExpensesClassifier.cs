using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class ExpensesClassifier
    {
        public ExpensesClassifier(IDictionary<string, string> keysCategories)
        {
            if (keysCategories == null)
            {
                keysCategories = new Dictionary<string, string>();
            }

            this.KeysCategories = keysCategories;
        }

        public IDictionary<string, string> KeysCategories { get; set; }

        public void Classify(IEnumerable<Expense> expenses)
        {
            foreach (var e in expenses)
            {
                var matchedCategory = this.KeysCategories.FirstOrDefault(x => e.Source.Contains(x.Key)).Value;
                if (matchedCategory != null)
                {
                    e.Category = matchedCategory;
                }
            }
        }
    }
}