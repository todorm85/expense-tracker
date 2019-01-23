using System;
using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public class Budget : IDataItem
    {
        public int Id { get; set; }

        public DateTime FromMonth { get; set; }

        public DateTime ToMonth { get; set; }

        public List<Transaction> ExpectedTransactions { get; set; }
    }
}