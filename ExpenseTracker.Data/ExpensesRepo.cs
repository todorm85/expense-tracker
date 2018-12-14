using System;
using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.Core;
using LiteDB;

namespace ExpenseTracker.Data
{
    public class ExpensesRepo : IExpensesRepository, IDisposable
    {
        private LiteDatabase db;
        private LiteCollection<Expense> context;

        public ExpensesRepo(string dbPath = @"Expenses.db")
        {
            this.db = new LiteDatabase(dbPath);
            this.context = this.db.GetCollection<Expense>("expenses");
        }

        public void Dispose()
        {
            this.db.Dispose();
        }

        public IEnumerable<Expense> GetAll()
        {
            var all = this.context.FindAll();
            return all;
        }

        public void Insert(IEnumerable<Expense> expenses)
        {
            this.context.Insert(expenses);
        }

        public void Update(Expense expense)
        {
            this.Update(new Expense[] { expense });
        }

        public void Update(IEnumerable<Expense> expenses)
        {
            var allExistingIds = this.GetAll().Select(x => x.Id);
            foreach (var expense in expenses)
            {
                if (expense.Id == 0 || !allExistingIds.Contains(expense.Id))
                {
                    throw new InvalidOperationException("Expense not found in database.");
                }
            }

            this.context.Update(expenses);
        }
    }
}
