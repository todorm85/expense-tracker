using ExpenseTracker.Core.Data;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace ExpenseTracker.Web.Pages.Shared
{
    public class PaginatedList<T> : List<T> where T: class
    {
        public PaginatedList()
        {
        }

        public PaginatedList(IGenericRepository<T> service, Expression<Func<T, bool>> filter, int pageIndex, int pageSize)
        {
            var allItemsCount = service.Count(filter);
            this.TotalPagesCount = (int)Math.Ceiling(allItemsCount / (double)pageSize);
            if (pageIndex >= this.TotalPagesCount)
                pageIndex = TotalPagesCount - 1;

            this.PageIndex = pageIndex;
            this.AddRange(service.GetAll(filter, pageIndex * pageSize, pageSize));
        }

        public int TotalPagesCount { get; }
        public int PageIndex { get; }
    }
}
