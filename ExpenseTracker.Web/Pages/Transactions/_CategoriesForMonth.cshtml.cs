using ExpenseTracker.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class CategoriesForMonthModel : IEnumerable<TransactionsForCategoryModel>
    {
        private const string UnspecifiedCategoryKeyName = "unspecified";
        private decimal? totalExpenses;
        private decimal? totalIncome;
        private IList<TransactionsForCategoryModel> transactionsByCateories = new List<TransactionsForCategoryModel>();

        public CategoriesForMonthModel(DateTime month)
        {
            this.Month = month;
        }

        public decimal Balance => this.TotalIncome - this.TotalExpenses;
        public int Count => this.transactionsByCateories.Count;
        public DateTime Month { get; private set; }

        public decimal TotalExpenses
        {
            get
            {
                if (!this.totalExpenses.HasValue)
                    this.totalExpenses = this.transactionsByCateories.Sum(x => x.TotalExpense);
                return this.totalExpenses.Value;
            }
        }

        public decimal TotalIncome
        {
            get
            {
                if (!this.totalIncome.HasValue)
                    this.totalIncome = this.transactionsByCateories.Sum(x => x.TotalIncome);
                return this.totalIncome.Value;
            }
        }

        public TransactionsForCategoryModel this[int index] { get => this.transactionsByCateories[index]; set => this.transactionsByCateories[index] = value; }

        public IEnumerator<TransactionsForCategoryModel> GetEnumerator()
        {
            foreach (var item in this.transactionsByCateories)
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void OrderCategories()
        {
            this.transactionsByCateories = this.transactionsByCateories.OrderByDescending(x => x.TotalExpense).ToList();
        }

        public static string GetCategoryKey(string currentCategory)
        {
            currentCategory = string.IsNullOrEmpty(currentCategory) ? UnspecifiedCategoryKeyName : currentCategory;
            return currentCategory;
        }

        internal void AddTransaction(Transaction t)
        {
            var transactionsForCategory = this.transactionsByCateories.FirstOrDefault(x => x.CategoryName == t.Category);
            if (transactionsForCategory == null)
            {
                transactionsForCategory = new TransactionsForCategoryModel(t.Category);
                this.transactionsByCateories.Add(transactionsForCategory);
            }

            transactionsForCategory.Add(t);
        }
    }
}
