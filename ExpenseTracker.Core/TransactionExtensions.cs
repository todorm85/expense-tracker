namespace ExpenseTracker.Core
{
    public static class TransactionExtensions
    {
        public static bool IsSame(this Transaction t, Transaction other)
        {
            return !t.IsManuallyCreated() && t.TransactionId == other.TransactionId && t.Amount == other.Amount;
        }

        public static bool IsManuallyCreated(this Transaction t)
        {
            return string.IsNullOrWhiteSpace(t.TransactionId);
        }
    }
}