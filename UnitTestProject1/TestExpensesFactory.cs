using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using ExpenseTracker.Core;

namespace ExpenseTracker.Tests
{
    class TestExpensesFactory
    {
        public static IEnumerable<Transaction> GetTestExpenses(int count)
        {
            var expenses = new List<Transaction>();
            for (int i = 0; i < count; i++)
            {
                expenses.Add(GetTestExpense());
            }

            return expenses;
        }

        public static Transaction GetTestExpense(DateTime? date = null, string category = null)
        {
            var random = new Random();
            var newDate = date != null ? DateTime.SpecifyKind(date.Value, DateTimeKind.Utc) : DateTime.Now.AddDays(random.Next(0, 10) * -1);
            return new Transaction()
            {
                Account = Guid.NewGuid().ToString(),
                Amount = (decimal)(random.NextDouble() * 100),
                Date = newDate,
                Details = Guid.NewGuid().ToString(),
                TransactionId = Guid.NewGuid().ToString(),
                Category = category ?? Guid.NewGuid().ToString()
            };
        }
    }
}
