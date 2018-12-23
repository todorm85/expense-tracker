using System;
using ExpenseTracker.Core;
using LiteDB;

namespace ExpenseTracker.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        public UnitOfWork(string dbPath)
        {
            this.db = new LiteDatabase(dbPath);
        }

        public IGenericRepository<Expense> Expenses
        {
            get
            {
                if (this.expenses == null)
                {
                    this.expenses = new GenericRepo<Expense>(this.db, "expenses");
                }

                return this.expenses;
            }
        }

        public IGenericRepository<Category> Categories
        {
            get
            {
                if (this.categories == null)
                {
                    this.categories = new GenericRepo<Category>(this.db, "categories");
                }

                return this.categories;
            }
        }

        public IGenericRepository<Budget> Budgets
        {
            get
            {
                if (this.budgets == null)
                {
                    this.budgets = new GenericRepo<Budget>(this.db, "budgets");
                }

                return this.budgets;
            }
        }

        public void Dispose()
        {
            this.db.Dispose();
        }

        public IGenericRepository<Expense> expenses;
        public IGenericRepository<Category> categories;
        private LiteDatabase db;
        private IGenericRepository<Budget> budgets;
    }
}