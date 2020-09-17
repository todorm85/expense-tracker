using System.Collections;
using System.Collections.Generic;
using ExpenseTracker.Core;

namespace ExpenseTracker.Web.Models.Transactions
{
    public class CategoryTransactions
    {
        public string CategoryName { get; set; }
        public bool IsExpanded { get; set; }
        public IEnumerable<Transaction> Transactions { get; set; }
    }
}