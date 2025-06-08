using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core.Services;

public class ItemsFilterResult<T> where T : class
{
    public IEnumerable<T> Items { get; set; } = Enumerable.Empty<T>();
    public int PagesCount { get; set; }
}
