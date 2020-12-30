﻿using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions;
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

        public int DetailsHeight { get; set; }
        public bool HideHeader { get; set; }
        public bool ShowId { get; set; }
        public bool ShowTime { get; set; }
        public IList<Transaction> Transactions { get; set; }
    }
}