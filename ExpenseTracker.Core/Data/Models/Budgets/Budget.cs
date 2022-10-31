using ExpenseTracker.Core.Transactions;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.Core.Budget
{
    public class Budget
    {
        public Budget()
        {
            this.ExpectedTransactions = new List<Transaction>();
        }

        public List<Transaction> ExpectedTransactions { get; set; }
        public DateTime FromMonth { get; set; }
        public int Id { get; set; }
        public DateTime ToMonth { get; set; }
    }
}