using System;
using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public interface ITransactionsService : IBaseDataItemService<Transaction>
    {
        void Classify();
        Dictionary<DateTime, Dictionary<string, IEnumerable<Transaction>>> GetExpensesByCategoriesByMonths(DateTime fromDate, DateTime toDate);
    }
}