using ExpenseTracker.Core.Transactions;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class ExpandableCategoryModel
    {
        private string categoryName;
        private decimal? totalExpense;
        private decimal? totalIncome;

        public ExpandableCategoryModel(string categoryName)
        {
            this.categoryName = categoryName;
            this.TransactionsList = new TransactionsListModel() { HideHeader = true };
            this.Categories = new List<ExpandableCategoryModel>();
        }

        public decimal Balance => this.TotalIncome - this.TotalExpense;
        public string CategoryName => categoryName;
        public int Count => this.TransactionsList.Transactions.Count;
        public bool IsNegativeBalance => this.Balance < 0;
        public string Id { get; set; }

        public decimal TotalExpense
        {
            get
            {
                if (!this.totalExpense.HasValue)
                    this.totalExpense = this.TransactionsList.Transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                return this.totalExpense.Value;
            }
        }

        public decimal TotalIncome
        {
            get
            {
                if (!this.totalIncome.HasValue)
                    this.totalIncome = this.TransactionsList.Transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                return this.totalIncome.Value;
            }
        }

        public TransactionsListModel TransactionsList { get; set; }

        public IList<ExpandableCategoryModel> Categories { get; set; }

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
