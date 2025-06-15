using System;
using System.Linq;
using System.Linq.Expressions;
using ExpenseTracker.Core.Data;

namespace ExpenseTracker.Core.Services;

public class ItemsFilterService<TItem, TResult, TParams>
    where TItem : class
    where TResult : ItemsFilterResult<TItem>, new()
    where TParams : ItemsFilterParams<TItem>, new()
{
    protected IReadRepository<TItem> repository;

    public ItemsFilterService(IReadRepository<TItem> repository)
    {
        this.repository = repository;
    }

    public virtual TResult GetFilteredItems(TParams filterParams = null)
    {
        filterParams = filterParams ?? new TParams();
        var totalCount = repository.Count(filterParams.Filter);

        var result = new TResult();
        var pagesCount = (int)Math.Ceiling(totalCount / (double)filterParams.PageSize);
        result.PagesCount = pagesCount;
        if (filterParams.PageIndex >= pagesCount)
        {
            filterParams.PageIndex = pagesCount - 1;
            result.PageIndex = filterParams.PageIndex;
        }

        var items = repository.GetAll(filter: filterParams.Filter, skip: filterParams.PageIndex * filterParams.PageSize, limit: filterParams.PageSize, orderBy: filterParams.OrderBy, ascending: !filterParams.Descending);
        result.Items = items;
        
        return result;
    }
}
