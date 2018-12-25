using System;
using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public class Budget : IDataItem
    {
        public int Id { get; set; }

        public DateTime Month { get; set; }

        public decimal ExpectedIncome { get; set; }

        public decimal ActualIncome { get; set; }

        public Dictionary<string, decimal> ExpectedExpensesByCategory { get; set; }
    }
}