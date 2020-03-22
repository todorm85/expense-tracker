using ExpenseTracker.Core;

namespace ExpenseTracker.Allianz
{
    public interface ITransactionBuilder
    {
        void Build(Transaction expense);
    }
}