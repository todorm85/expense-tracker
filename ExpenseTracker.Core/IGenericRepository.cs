using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public interface IGenericRepository<T> 
        where T : IDataItem
    {
        IEnumerable<T> GetAll();

        void Insert(IEnumerable<T> items);

        void Update(IEnumerable<T> items);
        void Remove(T item);
    }
}