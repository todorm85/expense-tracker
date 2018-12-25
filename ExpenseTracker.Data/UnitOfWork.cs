using System;
using System.Collections.Generic;
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

        public IGenericRepository<T> GetDataItemsRepo<T>() where T : IDataItem
        {
            if (this.repos.ContainsKey(typeof(T)))
            {
                return this.repos[typeof(T)] as IGenericRepository<T>;
            }

            var type = typeof(GenericRepo<T>);

            var ctor = type.GetConstructor(new Type[] { typeof(LiteDatabase), typeof(string) });
            GenericRepo<T> obj = default(GenericRepo<T>);
            if (ctor != null)
            {
                obj = ctor.Invoke(new object[] { this.db, this.GetSetName<T>() }) as GenericRepo<T>;
            }

            this.repos.Add(typeof(T), obj);

            return obj;
        }

        public void Dispose()
        {
            this.db.Dispose();
        }

        private string GetSetName<T>() where T : IDataItem
        {
            // backward compatability
            if (typeof(T) == typeof(Category))
            {
                return "categories";
            }
            else
            {
                return $"{typeof(T).Name.ToLower()}s";
            }
        }

        public IGenericRepository<Expense> expenses;
        public IGenericRepository<Category> categories;
        private LiteDatabase db;
        private IGenericRepository<Budget> budgets;
        private IDictionary<Type, object> repos;
    }
}