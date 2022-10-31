using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Rules;
using System;
using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Services.Models;

namespace ExpenseTracker.Web.Pages.Shared
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

        public CreateTransactionResult.Reason Reason { get; set; }
    }

    public class TransactionsListModel
    {
        public TransactionsListModel(IList<TransactionModel> transactions)
        {
            Transactions = transactions;
            DetailsHeight = 1;
        }

        public TransactionsListModel() : this(new List<TransactionModel>())
        {
        }

        public int DetailsHeight { get; set; }
        public bool HasFailed { get => Transactions.Any(t => t.Reason != CreateTransactionResult.Reason.None); }
        public bool HideHeader { get; set; }
        public bool ShowId { get; set; }
        public bool ShowTime { get; set; }
        public bool ShowSource { get; set; }
        public IList<TransactionModel> Transactions { get; set; }
    }
}