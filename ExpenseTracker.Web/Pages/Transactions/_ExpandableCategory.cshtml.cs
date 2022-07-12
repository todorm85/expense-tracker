using ExpenseTracker.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class ExpandableCategoryModel
    {
        private string categoryName;
        private string backgroundColor;

        public ExpandableCategoryModel(string categoryName)
        {
            this.categoryName = categoryName;
            this.TransactionsList = new TransactionsListModel() { HideHeader = true };
            this.Categories = new List<ExpandableCategoryModel>();
            this.BackgroundLightness = 80;
        }

        public decimal Balance => this.TotalIncome - this.TotalExpense;
        public string CategoryName => categoryName;
        public int Count => this.TransactionsList.Transactions.Count;
        public bool IsNegativeBalance => this.Balance < 0;
        public string ClientId
        {
            get
            {
                var result = ExpandableMonthModel.GetCategoryKey(this.CategoryName);
                if (this.ClientIdPrefix != null)
                {
                    result = this.ClientIdPrefix + "__" + result;
                }

                return result;
            }
        }

        public string ClientIdPrefix { get; set; }

        public decimal TotalExpense { get; set; }

        public decimal TotalIncome { get; set; }

        public TransactionsListModel TransactionsList { get; set; }

        public IList<ExpandableCategoryModel> Categories { get; set; }

        public string BackgroundColor
        {
            get
            {
                if (this.backgroundColor == null)
                {
                    this.backgroundColor = $"hsl({45}deg, {0}%, {this.BackgroundLightness}%)";
                }

                return this.backgroundColor;
            }
            set
            {
                this.backgroundColor = value;
            }
        }

        public int BackgroundLightness { get; set; }

        public Transaction this[int index] { get => this.TransactionsList.Transactions[index]; set => this.TransactionsList.Transactions[index] = new TransactionModel(value); }

        public void Add(Transaction item)
        {
            this.TransactionsList.Transactions.Add(new TransactionModel(item));
        }

        public bool Remove(Transaction item)
        {
            return this.TransactionsList.Transactions.Remove(new TransactionModel(item));
        }

        internal void OrderTransactions()
        {
            this.TransactionsList.Transactions = this.TransactionsList.Transactions.OrderByDescending(x => x.Amount).ToList();
        }
    }
}
