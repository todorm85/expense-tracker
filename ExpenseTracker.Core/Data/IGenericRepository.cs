using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpenseTracker.Core.Data
{
    public interface IGenericRepository<T> where T : class
    {
        int Count(Expression<Func<T, bool>> expression = null);

        IEnumerable<T> GetAll(Expression<Func<T, bool>> filter = null, int skip = 0, int take = int.MaxValue);

        T GetById(object id);

        void Insert(T item);

        void Insert(IEnumerable<T> items);

        void RemoveById(object id);

        void Update(IEnumerable<T> items);

        void Update(T item);
    }
}