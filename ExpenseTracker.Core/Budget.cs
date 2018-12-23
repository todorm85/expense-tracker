using System;
using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public class Budget : IDataItem
    {
        public Budget(int year, int month)
        {
            this.Month = new DateTime(year, month, 1);
        }

        public int Id { get; set; }

        public DateTime Month { get; set; }

        public decimal ExpectedIncome { get; set; }

        public Dictionary<string, decimal> ExpectedExpensesByCategory { get; set; }
    }
}