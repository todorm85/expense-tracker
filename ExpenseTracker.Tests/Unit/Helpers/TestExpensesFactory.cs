using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.Tests
{
    internal class TestExpensesFactory
    {
        public static Transaction GetTestExpense(DateTime? date = null, string category = null)
        {
            var random = new Random();
            var newDate = date != null ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc) : DateTime.Now.AddDays(random.Next(0, 10) * -1);
            return new Transaction()
            {
                Amount = (decimal)(random.NextDouble() * 100),
                Date = newDate,
                Details = Guid.NewGuid().ToString(),
                Category = category ?? Guid.NewGuid().ToString(),
                TransactionId = Guid.NewGuid().ToString(),
                Type = TransactionType.Expense,
                Source = "tests"
            };
        }

        public static IEnumerable<Transaction> GetTestExpenses(int count)
        {
            var expenses = new List<Transaction>();
            for (int i = 0; i < count; i++)
            {
                expenses.Add(GetTestExpense());
            }

            return expenses;
        }
    }
}