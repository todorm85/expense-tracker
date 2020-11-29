using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using ExpenseTracker.Core;
using LiteDB;

namespace ExpenseTracker.Data
{
    public class GenericRepo<T> : IGenericRepository<T>
        where T : IDataItem
    {
        private LiteDatabase db;
        private LiteCollection<T> context;

        public GenericRepo(LiteDatabase db, string collection)
        {
            this.db = db;
            this.context = this.db.GetCollection<T>(collection);
            if (typeof(T) == typeof(Transaction))
            {
                var transactionsContext = this.context as LiteCollection<Transaction>;
                transactionsContext.EnsureIndex(x => x.Date);
                transactionsContext.EnsureIndex(x => x.Amount);
                transactionsContext.EnsureIndex(x => x.TransactionId);
            }

            if (typeof(T) == typeof(Category))
            {
                var transactionsContext = this.context as LiteCollection<Category>;
                transactionsContext.EnsureIndex(x => x.KeyWord);
                transactionsContext.EnsureIndex(x => x.Name);
            }
        }

        public virtual IEnumerable<T> GetAll()
        {
            var all = this.context.FindAll();
            return all;
        }

        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            var all = this.context.Find(predicate);
            return all;
        }

        public virtual void Insert(IEnumerable<T> items)
        {
            this.context.Insert(items);
        }

        public virtual void Update(IEnumerable<T> items)
        {
            this.context.Update(items);
        }

        public virtual void Remove(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.context.Delete(item.Id);
            }
        }

        public void Remove(int id)
        {
            this.context.Delete(id);
        }

        public void Update(T item)
        {
            this.context.Update(item);
        }

        public void Insert(T item)
        {
            this.context.Insert(item);
        }
    }
}
