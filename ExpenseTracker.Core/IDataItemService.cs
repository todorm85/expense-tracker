using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public interface IDataItemService<T> where T : IDataItem
    {
        void Add(IEnumerable<T> items);

        void Remove(IEnumerable<T> items);

        void Update(IEnumerable<T> items);

        IEnumerable<T> GetAll();
    }
}