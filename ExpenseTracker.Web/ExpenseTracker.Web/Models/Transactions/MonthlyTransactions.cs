using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core;

namespace ExpenseTracker.Web.Models.Transactions
{
    public class MonthlyTransactions
    {
        public DateTime Date { get; set; }
        public bool IsExpanded { get; set; }
        public IEnumerable<CategoryTransactions> CategoriesTransactions { get; set; }
    }
}
