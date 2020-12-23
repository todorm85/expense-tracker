using System;
using System.Diagnostics;

namespace ExpenseTracker.Core
{
    [DebuggerDisplay("{Date.Month}|{Category}|{Amount}")]
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

        public override bool Equals(object obj) => this.Id == ((Transaction)obj).Id;
        public override int GetHashCode() => this.Id;
    }
}
