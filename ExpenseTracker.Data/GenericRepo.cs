using ExpenseTracker.Core;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpenseTracker.Data
{
    public class GenericRepo<T> : IGenericRepository<T>
        where T : class
    {
        private LiteCollection<T> context;
        private LiteDatabase db;

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
            return all.AsEnumerable();
        }

        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            var all = this.context.Find(predicate);
            return all.AsEnumerable();
        }

        public virtual T GetById(object id)
        {
            var all = this.context.FindById(new BsonValue(id));
            return all;
        }

        public virtual void Insert(IEnumerable<T> items)
        {
            this.context.Insert(items);
        }

        public virtual void Insert(T item)
        {
            this.context.Insert(item);
        }

        public virtual void RemoveById(object id)
        {
            this.context.Delete(new BsonValue(id));
        }

        public virtual void Update(IEnumerable<T> items)
        {
            this.context.Update(items);
        }

        public virtual void Update(T item)
        {
            this.context.Update(item);
        }
    }
}