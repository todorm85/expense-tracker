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
        public static IEnumerable<Expense> GetTestExpenses(int count)
        {
            var expenses = new List<Expense>();
            for (int i = 0; i < count; i++)
            {
                expenses.Add(GetTestExpense());
            }

            return expenses;
        }

        public static Expense GetTestExpense(DateTime? date = null, string category = null)
        {
            var random = new Random();

            return new Expense()
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
