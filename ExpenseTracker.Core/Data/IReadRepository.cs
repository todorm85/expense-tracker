using System.Collections.Generic;
using System.Linq.Expressions;
using System;

namespace ExpenseTracker.Core.Data
{
    public interface IReadRepository<T> where T : class
    {
        IEnumerable<T> GetAll(
            Expression<Func<T, bool>> filter = null, 
            int skip = 0, 
            int limit = int.MaxValue,
            Expression<Func<T, object>> orderBy = null,
            bool ascending = true);

        int Count(Expression<Func<T, bool>> expression = null);
    }
}