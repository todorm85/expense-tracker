using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpenseTracker.Core
{
    public interface IBaseDataItemService<T>
    {
        void Add(IEnumerable<T> items);

        void Remove(IEnumerable<T> items);

        void Update(IEnumerable<T> items);

        IEnumerable<T> GetAll();

        IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate);

        void Remove(int id);

        void Update(T item);

        void Add(T item);
    }
}