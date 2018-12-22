using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Core
{
    public class Expense : IDataItem
    {
        public int Id { get; set; }
        public decimal Amount  { get; set; }
        public string Source { get; set; }
        public DateTime Date { get; set; }
        public string Account { get; set; }
        public string TransactionId { get; set; }
        public string Category { get; set; }
    }
}
