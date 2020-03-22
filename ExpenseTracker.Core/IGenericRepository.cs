using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpenseTracker.Core
{
    public interface IGenericRepository<T> 
        where T : IDataItem
    {
        IEnumerable<T> GetAll();
        IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate);

        void Insert(IEnumerable<T> items);

        void Update(IEnumerable<T> items);
        void Remove(IEnumerable<T> items);
    }
}