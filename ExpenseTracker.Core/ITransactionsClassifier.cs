using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public interface ITransactionsClassifier
    {
        void Classify(IEnumerable<Transaction> filtered);
    }
}