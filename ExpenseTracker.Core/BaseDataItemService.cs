using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpenseTracker.Core
{
    public abstract class BaseDataItemService<T> : IBaseDataItemService<T> where T : class
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

        public virtual void Update(IEnumerable<T> items)
        {
            this.repo.Update(items);
        }

        public virtual IEnumerable<T> GetAll()
        {
            return this.repo.GetAll();
        }

        public virtual IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate)
        {
            return this.repo.GetAll(predicate);
        }

        public void RemoveById(object id)
        {
            this.repo.RemoveById(id);
        }

        public void Update(T item)
        {
            this.repo.Update(item);
        }

        public void Add(T item)
        {
            this.repo.Insert(item);
        }

        public T GetById(object id)
        {
            return this.repo.GetById(id);
        }
    }
}