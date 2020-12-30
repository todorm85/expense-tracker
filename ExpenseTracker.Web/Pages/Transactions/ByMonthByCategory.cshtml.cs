using ExpenseTracker.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class CategoriesForMonthModel : IEnumerable<TransactionsForCategoryModel>
    {
        private decimal? totalExpenses;
        private decimal? totalIncome;
        private IList<TransactionsForCategoryModel> transactionsByCateories = new List<TransactionsForCategoryModel>();

        public CategoriesForMonthModel(DateTime month)
        {
            this.Month = month;
        }

        public decimal Balance => this.TotalIncome - this.TotalExpenses;
        public int Count => this.transactionsByCateories.Count;
        public bool IsExpanded { get; set; }
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

    public class TransactionsByMonthByCategoryModel : PageModel
    {
        private const int initialMonthsBack = -1;
        private const string UnspecifiedCategoryKeyName = "unspecified";
        private readonly ITransactionsService transactionsService;

        public TransactionsByMonthByCategoryModel(
            ITransactionsService transactionsService)
        {
            this.AverageAndTotalsForCategory = new Dictionary<string, decimal[]>();
            this.CategoriesForMonths = new List<CategoriesForMonthModel>();
            this.transactionsService = transactionsService;
            this.Filters = new FiltersModel(initialMonthsBack);
        }

        public IDictionary<string, decimal[]> AverageAndTotalsForCategory { get; set; }
        public decimal AverageBalance => this.AverageIncome - this.AverageExpense;
        public decimal AverageExpense { get; private set; }
        public decimal AverageIncome { get; private set; }
        public decimal Balance => this.TotalIncome - this.TotalExpense;
        public List<CategoriesForMonthModel> CategoriesForMonths { get; set; }

        [BindProperty]
        public FiltersModel Filters { get; set; }

        public decimal TotalExpense { get; private set; }
        public decimal TotalIncome { get; private set; }

        public string GetCategoryKey(string currentCategory)
        {
            currentCategory = string.IsNullOrEmpty(currentCategory) ? UnspecifiedCategoryKeyName : currentCategory;
            return currentCategory;
        }

        public void OnGet()
        {
            this.ModelState.Clear();
            var all = this.Filters.GetTransactionsFiltered(this.transactionsService);
            if (all.Count() == 0)
                return;

            foreach (var t in all.OrderByDescending(x => x.Date.ToMonthStart()).ThenBy(x => x.Category))
            {
                var categoriesForMonth = this.CategoriesForMonths.FirstOrDefault(x => x.Month == t.Date.ToMonthStart());
                if (categoriesForMonth == null)
                {
                    categoriesForMonth = new CategoriesForMonthModel(t.Date.ToMonthStart());
                    this.CategoriesForMonths.Add(categoriesForMonth);
                }

                categoriesForMonth.AddTransaction(t);
            }

            this.CategoriesForMonths.ForEach(x =>
            {
                x.OrderCategories();
                foreach (var category in x)
                {
                    var categoryKey = GetCategoryKey(category.CategoryName);
                    if (!this.AverageAndTotalsForCategory.ContainsKey(categoryKey))
                        this.AverageAndTotalsForCategory.Add(categoryKey, new decimal[] { 0, 0 });
                    this.AverageAndTotalsForCategory[categoryKey][1] = this.AverageAndTotalsForCategory[categoryKey][1] + category.Balance;
                }
            });

            foreach (var item in this.AverageAndTotalsForCategory)
            {
                item.Value[0] = item.Value[1] / this.CategoriesForMonths.Count;
            }

            this.AverageExpense = all.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount) / this.CategoriesForMonths.Count;
            this.TotalExpense = all.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);
            this.AverageIncome = all.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount) / this.CategoriesForMonths.Count;
            this.TotalIncome = all.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);
        }

        public void OnPost()
        {
            OnGet();
        }
    }

    public class TransactionsForCategoryModel
    {
        private string categoryName;
        private decimal? totalExpense;
        private decimal? totalIncome;

        public TransactionsForCategoryModel(string categoryName)
        {
            this.categoryName = categoryName;
            this.TransactionsList = new TransactionsListModel() { HideHeader = true };
        }

        public decimal Balance => this.TotalIncome - this.TotalExpense;
        public string CategoryName => categoryName;
        public int Count => this.TransactionsList.Transactions.Count;
        public bool IsExpanded { get; set; }
        public bool IsNegativeBalance => this.Balance < 0;

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
        public Transaction this[int index] { get => this.TransactionsList.Transactions[index]; set => this.TransactionsList.Transactions[index] = value; }

        public void Add(Transaction item)
        {
            this.TransactionsList.Transactions.Add(item);
        }

        public bool Remove(Transaction item)
        {
            return this.TransactionsList.Transactions.Remove(item);
        }
    }
}