using ExpenseTracker.Core;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
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
        }

        public virtual int Count(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate != null)
                return this.context.Count(predicate);
            else
                return this.context.Count();
        }

        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate = null, int skip = 0, int take = int.MaxValue)
        {
            if (predicate == null)
            {
                if (skip == 0 && take == int.MaxValue)
                    return this.context.FindAll();
                else
                    predicate = x => true;
            }
         
            return this.context.Find(predicate, skip, take).AsEnumerable();
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