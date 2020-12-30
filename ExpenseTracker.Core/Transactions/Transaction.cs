using System;
using System.Diagnostics;

namespace ExpenseTracker.Core.Transactions
{
    [DebuggerDisplay("{TransactionId}|{Date.Month}|{Category}|{Amount}")]
    public class Transaction
    {
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public string Details { get; set; }
        public bool Ignored { get; set; }
        public bool IsResolvedDuplicate { get; set; }
        public string TransactionId { get; set; }
        public TransactionType Type { get; set; }

        public override bool Equals(object obj) => string.IsNullOrEmpty(this.TransactionId) ? base.Equals(obj) : this.TransactionId == ((Transaction)obj).TransactionId;

        public override int GetHashCode() => string.IsNullOrEmpty(this.TransactionId) ? base.GetHashCode() : this.TransactionId.GetHashCode();
    }
}