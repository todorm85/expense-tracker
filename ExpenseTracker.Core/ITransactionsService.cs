using System;
using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public interface ITransactionsService : IBaseDataItemService<Transaction>
    {
        void Add(IEnumerable<Transaction> expenses, out IEnumerable<Transaction> added);

        IEnumerable<Transaction> GetDuplicates(Transaction t);

        List<List<Transaction>> GetPotentialDuplicates();
    }
}