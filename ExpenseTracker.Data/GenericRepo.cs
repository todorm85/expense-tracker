using ExpenseTracker.Core;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using LiteDB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text.RegularExpressions; // Added for Regex

namespace ExpenseTracker.Data
{
    public class GenericRepo<T> : IRepository<T>
        where T : class
    {
        private ILiteCollection<T> context;
        private LiteDatabase db;

        public GenericRepo(LiteDatabase db, string collection)
        {
            this.db = db;
            this.context = this.db.GetCollection<T>(collection);
            if (this.context is ILiteCollection<Transaction> transactionsContext)
            {
                transactionsContext.EnsureIndex(x => x.Date);
                transactionsContext.EnsureIndex(x => x.Amount);
                transactionsContext.EnsureIndex(x => x.TransactionId);
                transactionsContext.EnsureIndex(x => x.Category);
            }
        }

        public virtual int Count(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate != null)
                return this.GetAll(predicate).Count();
            else
                return this.context.Count();
        }        // The only thing needed to optimize for local db is filtering in order to protect against serialization of all db entries, everything else like ordering and skip and take should be done in memmory
        public virtual IEnumerable<T> GetAll(
            Expression<Func<T, bool>> predicate = null, 
            int skip = 0, 
            int limit = int.MaxValue,
            Expression<Func<T, object>> orderBy = null,
            bool ascending = true)
        {
            if (predicate == null)
            {
                var results = this.context.Find(Query.All(), skip, limit);
                if (orderBy != null)
                {
                    results = ascending ? 
                        results.AsQueryable().OrderBy(orderBy.Compile()).ToList() : 
                        results.AsQueryable().OrderByDescending(orderBy.Compile()).ToList();
                }
                return results;
            }

            try
            {
                // the filter I mostly used is not supported by LiteDb
                // return this.context.Find(predicate, skip, limit);

                // not best but does the job, most used predicate is not supported by LiteDB
                // no use trying those requests first only to throw exception and retry like this
                return FilterInMemory(predicate, skip, limit, orderBy, ascending);
            }
            catch (NotSupportedException)
            {
                return FilterInMemory(predicate, skip, limit, orderBy, ascending);
            }

            IEnumerable<T> FilterInMemory(
                Expression<Func<T, bool>> predicate, 
                int skip, 
                int limit,
                Expression<Func<T, object>> orderBy = null,
                bool ascending = true)
            {
                var all = this.context.FindAll();
                var filteredResult = all.Where(predicate.Compile());
                
                // Apply ordering if specified
                if (orderBy != null)
                {
                    filteredResult = ascending ? 
                        filteredResult.OrderBy(orderBy.Compile()) : 
                        filteredResult.OrderByDescending(orderBy.Compile());
                }
                
                // Apply pagination
                if ((skip > 0) || (limit != int.MaxValue))
                    filteredResult = filteredResult.Skip(skip).Take(limit);
                
                return filteredResult;
            }
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