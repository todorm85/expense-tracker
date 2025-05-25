using System.Collections.Generic;
using ExpenseTracker.Core.Services.Models;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Integrations
{
    public class ImportResult
    {
        public IEnumerable<Transaction> Added { get; set; }
        public IEnumerable<CreateTransactionResult> Skipped { get; set; }
        public string Error { get; set; }
    }
}
