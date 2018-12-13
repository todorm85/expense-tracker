using System;
using System.Collections.Generic;
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
            this.context = db.GetCollection<Expense>("expenses");
        }

        public void Dispose()
        {
            this.db.Dispose();
        }

        public IEnumerable<Expense> GetAll()
        {
            return context.FindAll();
        }

        public void Insert(IEnumerable<Expense> expenses)
        {
            context.Insert(expenses);
        }

        public void Update(IEnumerable<Expense> expenses)
        {
            context.Update(expenses);
        }
    }
}
