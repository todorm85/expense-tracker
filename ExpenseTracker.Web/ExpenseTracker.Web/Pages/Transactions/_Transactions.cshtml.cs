using ExpenseTracker.Core;
using System.Collections.Generic;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class TransactionsListModel
    {
        public TransactionsListModel()
        {
            this.Transactions = new List<Transaction>();
            this.DetailsHeight = 2;
        }

        public bool ShowId { get; set; }
        public bool ShowTime { get; set; }
        public int DetailsHeight { get; set; }
        public bool HideHeader { get; set; }
        public IList<Transaction> Transactions { get; set; }
    }
}
