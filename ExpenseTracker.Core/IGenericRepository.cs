using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpenseTracker.Core
{
    public interface IGenericRepository<T>
        where T : class
    {
        IEnumerable<T> GetAll();

        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter);

        T GetById(object id);

        void Insert(T item);

        void Insert(IEnumerable<T> items);

        void RemoveById(object id);

        void Update(T item);

        void Update(IEnumerable<T> items);
    }
}