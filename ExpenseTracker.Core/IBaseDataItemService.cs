using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public interface IBaseDataItemService<T>
    {
        void Add(IEnumerable<T> items);

        void Remove(IEnumerable<T> items);

        void Update(IEnumerable<T> items);

        IEnumerable<T> GetAll();
    }
}