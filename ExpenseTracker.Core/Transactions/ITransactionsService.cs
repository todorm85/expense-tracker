using ExpenseTracker.Core.Data;
using System.Collections.Generic;

namespace ExpenseTracker.Core.Transactions
{
    public interface ITransactionsService : IGenericRepository<Transaction>
    {
        List<List<Transaction>> GetPotentialDuplicates();

        bool TryAdd(IEnumerable<Transaction> expenses, out IEnumerable<TransactionInsertResult> skipped);

        bool TryAdd(Transaction t, out IEnumerable<TransactionInsertResult> skipped);
    }
}