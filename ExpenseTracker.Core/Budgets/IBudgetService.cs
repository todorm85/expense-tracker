using ExpenseTracker.Core.Data;
using System;

namespace ExpenseTracker.Core.Budget
{
    public interface IBudgetService : IGenericRepository<Budget>
    {
        Budget GetCumulativeForMonth(DateTime month);
    }
}