using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpenseTracker.Core.Data
{
    public interface IBaseDataItemService<T>
    {
        void Add(IEnumerable<T> items);

        void Add(T item);

        IEnumerable<T> GetAll();

        IEnumerable<T> GetAll(Expression<Func<T, bool>> predicate);

        T GetById(object id);

        void RemoveById(object id);

        void Update(IEnumerable<T> items);

        void Update(T item);
    }
}