using System;
using System.Collections.Generic;
using System.Text;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Integrations
{
    public class ImportResult
    {
        public IEnumerable<Transaction> Added { get; set; }
        public IEnumerable<Transaction> Skipped { get; set; }
        public string Error { get; set; }
    }
}
