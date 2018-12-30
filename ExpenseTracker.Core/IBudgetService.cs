using System;

namespace ExpenseTracker.Core
{
    public interface IBudgetService : IBaseDataItemService<Budget>
    {
        Budget GetByMonth(DateTime month);
    }
}