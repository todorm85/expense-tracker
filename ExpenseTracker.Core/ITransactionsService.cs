using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace ExpenseTracker.Core
{
    public interface ITransactionsService : IBaseDataItemService<Transaction>
    {
        bool TryAdd(IEnumerable<Transaction> expenses, out IEnumerable<TransactionInsertResult> skipped);
        bool TryAdd(Transaction t, out IEnumerable<TransactionInsertResult> skipped);
        List<List<Transaction>> GetPotentialDuplicates();
    }
}