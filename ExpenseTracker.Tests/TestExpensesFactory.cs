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
            var random = new Random();
            for (int i = 0; i < count; i++)
            {
                expenses.Add(new Expense()
                {
                    Account = Guid.NewGuid().ToString(),
                    Amount = (decimal)(random.NextDouble() * 100),
                    Date = DateTime.Now.AddDays(random.Next(0, 10) * -1),
                    Source = Guid.NewGuid().ToString(),
                    TransactionId = Guid.NewGuid().ToString(),
                    Category = Guid.NewGuid().ToString()
                });
            }

            return expenses;
        }
    }
}
