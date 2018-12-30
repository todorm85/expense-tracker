using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public abstract class BaseDataItemService<T> : IBaseDataItemService<T> where T : IDataItem
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

        public virtual void Remove(IEnumerable<T> items)
        {
            this.repo.Remove(items);
        }

        public virtual void Update(IEnumerable<T> items)
        {
            this.repo.Update(items);
        }

        public virtual IEnumerable<T> GetAll()
        {
            return this.repo.GetAll();
        }
    }
}