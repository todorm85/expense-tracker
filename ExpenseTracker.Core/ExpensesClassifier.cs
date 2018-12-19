using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    internal class ExpensesClassifier
    {
        public ExpensesClassifier() : this(null)
        {
        }

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
                KeyValuePair<string, string> matchedCategory = this.KeysCategories.FirstOrDefault(x => e.Source != null && !string.IsNullOrWhiteSpace(x.Key) && e.Source.Contains(x.Key));
                if (!matchedCategory.Equals(default(KeyValuePair<string, string>)))
                {
                    e.Category = matchedCategory.Value;
                }
            }
        }
    }
}