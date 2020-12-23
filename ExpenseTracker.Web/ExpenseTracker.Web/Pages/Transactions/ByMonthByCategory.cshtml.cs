using ExpenseTracker.Core;
using ExpenseTracker.Web.Models.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class TransactionsByMonthByCategoryModel : PageModel
    {
        private const string UnspecifiedCategoryKeyName = "unspecified";
        private const int initialMonthsBack = -1;
        private readonly ITransactionsService transactionsService;

        public TransactionsByMonthByCategoryModel(
            ITransactionsService transactionsService)
        {
            this.AverageAndTotalsForCategory = new Dictionary<string, decimal[]>();
            this.CategoriesForMonths = new List<CategoriesForMonthModel>();
            this.transactionsService = transactionsService;
            this.Filters = new FiltersModel(initialMonthsBack);
        }

        public decimal AverageExpense { get; private set; }
        public decimal TotalExpense { get; private set; }
        public decimal AverageIncome { get; private set; }
        public decimal TotalIncome { get; private set; }

        public decimal AverageBalance => this.AverageIncome - this.AverageExpense;
        public decimal Balance => this.TotalIncome - this.TotalExpense;
        public IDictionary<string, decimal[]> AverageAndTotalsForCategory { get; set; }

        public List<CategoriesForMonthModel> CategoriesForMonths { get; set; }
        [BindProperty]
        public FiltersModel Filters { get; set; }

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

    public class CategoriesForMonthModel : IEnumerable<TransactionsForCategoryModel>
    {
        private decimal? totalExpenses;
        private decimal? totalIncome;
        private IList<TransactionsForCategoryModel> transactionsByCateories = new List<TransactionsForCategoryModel>();

        public CategoriesForMonthModel(DateTime month)
        {
            this.Month = month;
        }

        public TransactionsForCategoryModel this[int index] { get => this.transactionsByCateories[index]; set => this.transactionsByCateories[index] = value; }

        public DateTime Month { get; private set; }
        public bool IsExpanded { get; set; }
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

        public decimal Balance => this.TotalIncome - this.TotalExpenses;
        public int Count => this.transactionsByCateories.Count;

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

        public IEnumerator<TransactionsForCategoryModel> GetEnumerator()
        {
            foreach (var item in this.transactionsByCateories)
            {
                yield return item;
            }
        }

        public void OrderCategories()
        {
            this.transactionsByCateories = this.transactionsByCateories.OrderByDescending(x => x.TotalExpense).ToList();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }

    public class TransactionsForCategoryModel : IList<Transaction>
    {
        private string categoryName;
        private IList<Transaction> transactions = new List<Transaction>();
        private decimal? totalExpense;
        private decimal? totalIncome;

        public TransactionsForCategoryModel(string categoryName)
        {
            this.categoryName = categoryName;
        }

        public Transaction this[int index] { get => this.transactions[index]; set => this.transactions[index] = value; }

        public bool IsExpanded { get; set; }
        public decimal TotalExpense
        {
            get
            {
                if (!this.totalExpense.HasValue)
                    this.totalExpense = this.transactions.Where(t => t.Type == TransactionType.Expense).Sum(t => t.Amount);
                return this.totalExpense.Value;
            }
        }

        public decimal TotalIncome
        {
            get
            {
                if (!this.totalIncome.HasValue)
                    this.totalIncome = this.transactions.Where(t => t.Type == TransactionType.Income).Sum(t => t.Amount);
                return this.totalIncome.Value;
            }
        }

        public decimal Balance => this.TotalIncome - this.TotalExpense;

        public bool IsNegativeBalance => this.Balance < 0;
        public string CategoryName => categoryName;
        public int Count => this.transactions.Count;

        public bool IsReadOnly => this.transactions.IsReadOnly;

        public void Add(Transaction item)
        {
            this.transactions.Add(item);
        }

        public void Clear()
        {
            throw new NotImplementedException();
        }

        public bool Contains(Transaction item)
        {
            throw new NotImplementedException();
        }

        public void CopyTo(Transaction[] array, int arrayIndex)
        {
            throw new NotImplementedException();
        }

        public IEnumerator<Transaction> GetEnumerator()
        {
            foreach (var item in this.transactions)
            {
                yield return item;
            }
        }

        public int IndexOf(Transaction item)
        {
            throw new NotImplementedException();
        }

        public void Insert(int index, Transaction item)
        {
            throw new NotImplementedException();
        }

        public bool Remove(Transaction item)
        {
            return this.transactions.Remove(item);
        }

        public void RemoveAt(int index)
        {
            throw new NotImplementedException();
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }
    }
}
