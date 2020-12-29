using System;

namespace ExpenseTracker.Core
{
    public interface IBudgetService : IBaseDataItemService<Budget>
    {
        Budget GetCumulativeForMonth(DateTime month);
    }
}