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

            return new Transaction()
            {
                Account = Guid.NewGuid().ToString(),
                Amount = (decimal)(random.NextDouble() * 100),
                Date = date ?? DateTime.Now.AddDays(random.Next(0, 10) * -1),
                Source = Guid.NewGuid().ToString(),
                TransactionId = Guid.NewGuid().ToString(),
                Category = category ?? Guid.NewGuid().ToString()
            };
        }
    }
}
