using ExpenseTracker.Core.Data;
using ExpenseTracker.Web.Pages.Transactions;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ExpenseTracker.Web.Pages.Shared
{
    public class PaginatedList<T> : List<T> where T : class
    {
        public PaginatedList()
        {
        }

        public PaginatedList(IReadRepository<T> service, Expression<Func<T, bool>> filter, int pageIndex, int pageSize, string sortBy = null)
        {
            var allItemsCount = service.Count(filter);
            this.TotalPagesCount = (int)Math.Ceiling(allItemsCount / (double)pageSize);
            if (pageIndex >= this.TotalPagesCount)
                pageIndex = TotalPagesCount - 1;

            this.PageIndex = pageIndex;

            var result = service.GetAll(filter);
            if (!string.IsNullOrEmpty(sortBy))
            {
                var param = Expression.Parameter(typeof(T), sortBy);
                var sortExpression = Expression.Lambda<Func<T, object>>
                    (Expression.Convert(Expression.Property(param, sortBy), typeof(object)), param);
                result = result.AsQueryable().OrderBy(sortExpression);
            }

            this.AddRange(result.Skip(pageIndex * pageSize).Take(pageSize));
        }

        public int TotalPagesCount { get; }
        public int PageIndex { get; }
    }
}
