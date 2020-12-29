using System;
using System.Diagnostics;

namespace ExpenseTracker.Core
{
    [DebuggerDisplay("{Id}|{Date.Month}|{Category}|{Amount}")]
    public class Transaction
    {
        public decimal Amount { get; set; }
        public string Category { get; set; }
        public DateTime Date { get; set; }
        public string Details { get; set; }
        public int Id { get; set; }
        public bool Ignored { get; set; }
        public bool IsResolvedDuplicate { get; set; }
        public string TransactionId { get; set; }
        public TransactionType Type { get; set; }

        public override bool Equals(object obj) => this.Id == ((Transaction)obj).Id;

        public override int GetHashCode() => this.Id;
    }
}