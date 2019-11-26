using System;

namespace ExpenseTracker.Core
{
    public class Transaction : IDataItem
    {
        public int Id { get; set; }
        public decimal Amount { get; set; }
        public string Details { get; set; }
        public DateTime Date { get; set; }
        public string Category { get; set; }
        public TransactionType Type { get; set; }
        public bool Ignored { get; set; }
        public string TransactionId { get; set; }
    }
}
