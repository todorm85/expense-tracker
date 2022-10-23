using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Transactions.Rules;
using System;
using System.Collections.Generic;
using System.Linq;

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

        public TransactionInsertResult.Reason Reason { get; set; }

        internal void Update(ITransactionsService transactionsService, IGenericRepository<Rule> rules)
        {
            var dbModel = transactionsService.GetAll(x => x.TransactionId == TransactionId).First();
            if (Category != dbModel.Category && Category?.Contains(":") == true)
            {
                var parts = Category.Split(":");
                Category = parts[0];
                rules.Insert(new Rule() { ValueToSet = parts[0], ConditionValue = parts[1], Condition = RuleCondition.Contains, Action = RuleAction.SetProperty, Property = "Details", PropertyToSet = "Category" });
            }

            transactionsService.Update(new Transaction[] { ToDbModel() });
        }

        private Transaction ToDbModel()
        {
            Transaction dbModel = new Transaction();
            dbModel.TransactionId = TransactionId;
            dbModel.Details = Details;
            dbModel.Amount = Amount;
            dbModel.Date = Date;
            dbModel.Category = Category ?? "";
            dbModel.Type = Type;
            return dbModel;
        }
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
        public bool HasFailed { get => Transactions.Any(t => t.Reason != TransactionInsertResult.Reason.None); }
        public bool HideHeader { get; set; }
        public bool ShowId { get; set; }
        public bool ShowTime { get; set; }
        public bool ShowSource { get; set; }
        public IList<TransactionModel> Transactions { get; set; }
    }
}