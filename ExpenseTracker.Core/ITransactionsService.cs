using System;
using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public interface ITransactionsService : IBaseDataItemService<Transaction>
    {
        Dictionary<DateTime, Dictionary<string, IEnumerable<Transaction>>> GetExpensesByCategoriesByMonths(DateTime fromDate, DateTime toDate);
        IEnumerable<Transaction> GetDuplicates(Transaction t);
        void Add(IEnumerable<Transaction> expenses, out IEnumerable<Transaction> added);
    }
}