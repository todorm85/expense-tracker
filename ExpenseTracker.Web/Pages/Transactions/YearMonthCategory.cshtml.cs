using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class YearMonthCategoryModel : PageModel
    {
        private readonly ITransactionsService transactionsService;

        public YearMonthCategoryModel(
            ITransactionsService transactionsService)
        {
            this.AverageAndTotalsForCategory = new Dictionary<string, decimal[]>();
            this.ExpandableMonths = new List<ExpandableMonthModel>();
            this.transactionsService = transactionsService;
            this.Filters = new FiltersModel(transactionsService) { HideSorting = true };
        }

        public IDictionary<string, decimal[]> AverageAndTotalsForCategory { get; set; }
        public decimal AverageBalance => this.AverageIncome - this.AverageExpense;
        public decimal AverageExpense { get; private set; }
        public decimal AverageIncome { get; private set; }
        public decimal Balance => this.TotalIncome - this.TotalExpense;
        public List<ExpandableMonthModel> ExpandableMonths { get; set; }

        [BindProperty]
        public FiltersModel Filters { get; set; }

        public decimal TotalExpense { get; private set; }

        public decimal TotalIncome { get; private set; }

        public void OnGet()
        {
            this.ModelState.Clear();
            var all = this.Filters.GetTransactionsFiltered(transactionsService);
            if (all.Count() == 0)
                return;

            foreach (var t in all.OrderByDescending(x => x.Date.ToMonthStart()).ThenBy(x => x.Category))
            {
                var categoriesForMonth = this.ExpandableMonths.FirstOrDefault(x => x.Month == t.Date.ToMonthStart());
                if (categoriesForMonth == null)
                {
                    categoriesForMonth = new ExpandableMonthModel(t.Date.ToMonthStart());
                    this.ExpandableMonths.Add(categoriesForMonth);
                }

                categoriesForMonth.AddTransaction(t);
            }

            this.ExpandableMonths.ForEach(x =>
            {
                x.OrderCategories();
                foreach (var category in x)
                {
                    category.OrderTransactions();
                    var categoryKey = ExpandableMonthModel.GetCategoryKey(category.CategoryName);
                    if (!this.AverageAndTotalsForCategory.ContainsKey(categoryKey))
                        this.AverageAndTotalsForCategory.Add(categoryKey, new decimal[] { 0, 0 });
                    this.AverageAndTotalsForCategory[categoryKey][1] = this.AverageAndTotalsForCategory[categoryKey][1] + category.Balance;
                }
            });

            foreach (var item in this.AverageAndTotalsForCategory)
            {
                item.Value[0] = item.Value[1] / this.ExpandableMonths.Count;
            }

            this.AverageExpense = all.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount) / this.ExpandableMonths.Count;
            this.TotalExpense = all.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);
            this.AverageIncome = all.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount) / this.ExpandableMonths.Count;
            this.TotalIncome = all.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);
        }

        public void OnPost()
        {
            OnGet();
        }
    }
}