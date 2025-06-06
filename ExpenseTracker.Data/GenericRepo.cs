﻿using ExpenseTracker.Core;
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
        }

        // The only thing needed to optimize for local db is filtering in order to protect against serialization of all db entries, everything else like ordering and skip and take should be done in memmory
        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate = null)
        {
            if (predicate == null)
            {
                return this.context.FindAll();
            }

            return this.context.FindAll().ToList().Where(predicate.Compile());
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