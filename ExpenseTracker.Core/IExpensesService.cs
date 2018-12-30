using System;
using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public interface IExpensesService : IBaseDataItemService<Expense>
    {
        void Classify();
        Dictionary<DateTime, Dictionary<string, IEnumerable<Expense>>> GetExpensesByCategoriesByMonths(DateTime fromDate, DateTime toDate);
    }
}