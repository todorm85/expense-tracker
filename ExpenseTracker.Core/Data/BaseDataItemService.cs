using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpenseTracker.Core.Data
{
    public class BaseDataItemService<T> : IBaseDataItemService<T> where T : class
    {
        protected IGenericRepository<T> repo;

        public BaseDataItemService(IUnitOfWork uow)
        {
            this.repo = uow.GetDataItemsRepo<T>();
        }

        public virtual void Add(IEnumerable<T> items)
        {
            this.repo.Insert(items);
        }

        public virtual void Add(T item)
        {
            this.repo.Insert(item);
        }

        public virtual IEnumerable<T> GetAll()
        {
            return this.repo.GetAll();
        }

        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            return this.repo.GetAll(predicate);
        }

        public virtual T GetById(object id)
        {
            return this.repo.GetById(id);
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