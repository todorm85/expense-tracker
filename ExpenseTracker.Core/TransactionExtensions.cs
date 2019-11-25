namespace ExpenseTracker.Core
{
    public static class TransactionExtensions
    {
        // no way to identify transactions correctly from allianz sources
        public static bool IsSame(this Transaction t, Transaction other)
        {
            return t.Amount == other.Amount &&
                t.Date.Year == other.Date.Year &&
                t.Date.Month == other.Date.Month &&
                t.Date.Day == other.Date.Day;
        }
    }
}