using ExpenseTracker.Core.Data;
using System;

namespace ExpenseTracker.Core.Budget
{
    public interface IBudgetService : IBaseDataItemService<Budget>
    {
        Budget GetCumulativeForMonth(DateTime month);
    }
}