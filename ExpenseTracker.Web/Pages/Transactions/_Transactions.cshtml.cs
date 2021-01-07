using ExpenseTracker.Core.Transactions;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class TransactionModel : Transaction
    {
        public TransactionModel()
        {
        }

        public TransactionModel(Transaction t)
        {
            foreach (var p in t.GetType().GetProperties())
            {
                p.SetValue(this, p.GetValue(t));
            }
        }

        public TransactionInsertResult.Reason Reason { get; set; }
    }

    public class TransactionsListModel
    {
        public TransactionsListModel(IList<TransactionModel> transactions)
        {
            this.Transactions = transactions;
            this.DetailsHeight = 2;
        }

        public TransactionsListModel() : this(new List<TransactionModel>())
        {
        }

        public int DetailsHeight { get; set; }
        public bool HasFailed { get => this.Transactions.Any(t => t.Reason != TransactionInsertResult.Reason.None); }
        public bool HideHeader { get; set; }
        public bool ShowId { get; set; }
        public bool ShowTime { get; set; }
        public bool ShowSource { get; set; }
        public IList<TransactionModel> Transactions { get; set; }
    }
}