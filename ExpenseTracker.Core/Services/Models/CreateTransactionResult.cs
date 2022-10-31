using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Core.Services.Models
{
    public class CreateTransactionResult
    {
        public CreateTransactionResult()
        {

        }

        public CreateTransactionResult(Transaction t, Reason r)
        {
            Transaction = t;
            ReasonResult = r;
        }

        public enum Reason
        {
            None,
            InvalidDate,
            InvalidAmount,
            DuplicateEntry,
            InvalidType,
            Skipped,
            InvalidSource,
            InvalidId
        }

        public Reason ReasonResult { get; set; }
        public Transaction Transaction { get; set; }
    }
}
