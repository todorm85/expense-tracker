using System;
using System.Linq.Expressions;

namespace ExpenseTracker.Core.Services;

public class ItemsFilterParams<TItem> where TItem : class
{
    public Expression<Func<TItem, bool>> Filter { get; set; }

    public int PageSize { get; set; }

    public int PageIndex { get; set; }
}
