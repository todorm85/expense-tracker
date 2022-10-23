using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Transactions.Rules;
using System;
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

        internal void Update(ITransactionsService transactionsService, IGenericRepository<Rule> rules)
        {
            var dbModel = transactionsService.GetAll(x => x.TransactionId == this.TransactionId).First();
            if (this.Category != dbModel.Category && this.Category?.Contains(":") == true)
            {
                var parts = this.Category.Split(":");
                this.Category = parts[0];
                rules.Insert(new Rule() { ValueToSet = parts[0], ConditionValue = parts[1], Condition = RuleCondition.Contains, Action = RuleAction.SetProperty, Property = "Details", PropertyToSet = "Category" });
            }

            transactionsService.Update(new Transaction[] { this.ToDbModel() });
        }

        private Transaction ToDbModel()
        {
            Transaction dbModel = new Transaction();
            dbModel.TransactionId = this.TransactionId;
            dbModel.Details = this.Details;
            dbModel.Amount = this.Amount;
            dbModel.Date = this.Date;
            dbModel.Category = this.Category ?? "";
            dbModel.Type = this.Type;
            return dbModel;
        }
    }

    public class TransactionsListModel
    {
        public TransactionsListModel(IList<TransactionModel> transactions)
        {
            this.Transactions = transactions;
            this.DetailsHeight = 1;
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