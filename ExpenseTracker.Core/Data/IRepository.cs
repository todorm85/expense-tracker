using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpenseTracker.Core.Data
{
    public interface IRepository<T> : IReadRepository<T> where T : class
    {
        T GetById(object id);

        void Insert(T item);

        void Insert(IEnumerable<T> items);

        void RemoveById(object id);

        void Update(IEnumerable<T> items);

        void Update(T item);
    }
}