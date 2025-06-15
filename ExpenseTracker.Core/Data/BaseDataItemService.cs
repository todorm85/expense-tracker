using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpenseTracker.Core.Data
{
    public class BaseDataItemService<T> : IRepository<T> where T : class
    {
        protected IRepository<T> repo;

        public BaseDataItemService(IUnitOfWork uow)
        {
            this.repo = uow.GetDataItemsRepo<T>();
        }

        public int Count(Expression<Func<T, bool>> expression = null)
        {
            return this.repo.Count(expression);
        }        public IEnumerable<T> GetAll(
            Expression<Func<T, bool>> predicate = null, 
            int skip = 0, 
            int limit = int.MaxValue,
            Expression<Func<T, object>> orderBy = null,
            bool ascending = true)
        {
            return this.repo.GetAll(predicate, skip, limit, orderBy, ascending);
        }

        public virtual T GetById(object id)
        {
            return this.repo.GetById(id);
        }

        public virtual void Insert(IEnumerable<T> items)
        {
            this.repo.Insert(items);
        }

        public virtual void Insert(T item)
        {
            this.repo.Insert(item);
        }

        public virtual void RemoveById(object id)
        {
            this.repo.RemoveById(id);
        }

        public virtual void Update(IEnumerable<T> items)
        {
            this.repo.Update(items);
        }

        public virtual void Update(T item)
        {
            this.repo.Update(item);
        }
    }
}