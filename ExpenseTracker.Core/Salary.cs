namespace ExpenseTracker.Core
{
    internal class Salary : IDataItem
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
    }
}