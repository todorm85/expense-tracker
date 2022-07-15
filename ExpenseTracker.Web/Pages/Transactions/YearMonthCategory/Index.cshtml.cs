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
            this.CategorySummaries = new List<CategorySummary>();
            this.ExpandableMonths = new List<ExpandableMonthModel>();
            this.transactionsService = transactionsService;
            this.Filters = new FiltersModel(transactionsService) { HideSorting = true };
        }

        public IList<CategorySummary> CategorySummaries { get; set; }
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

            var allCategorySummaries = new List<CategorySummary>();
            this.ExpandableMonths.ForEach(expandableMonth =>
            {
                var categories = expandableMonth as IEnumerable<ExpandableCategoryModel>;
                expandableMonth.OrderMonthCategories();
                GetAllCategorySummaries(allCategorySummaries, categories);
            });

            foreach (var item in allCategorySummaries)
            {
                item.Average = item.Totals / this.ExpandableMonths.Count;
            }

            this.CategorySummaries = allCategorySummaries;

            this.AverageExpense = all.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount) / this.ExpandableMonths.Count;
            this.TotalExpense = all.Where(x => x.Type == TransactionType.Expense).Sum(x => x.Amount);
            this.AverageIncome = all.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount) / this.ExpandableMonths.Count;
            this.TotalIncome = all.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);
        }

        private static void GetAllCategorySummaries(List<CategorySummary> allCategorySummaries, IEnumerable<ExpandableCategoryModel> categories)
        {
            foreach (var expandableCategory in categories)
            {

            }
            var categoryKey = ExpandableMonthModel.GetCategoryKey(currentCatSum.CategoryName);
            var cs = allCategorySummaries.FirstOrDefault(cs => cs.Name == categoryKey);
            if (cs == null)
            {
                cs = new CategorySummary() { Name = currentCatSum.CategoryName };
                allCategorySummaries.Add(cs);
            }

            cs.Totals += currentCatSum.Balance;
        }

        public void OnPost()
        {
            OnGet();
        }
    }

    public class CategorySummary
    {
        public string Name { get; set; }
        public decimal Average { get; set; }
        public decimal Totals { get; set; }
        public IList<CategorySummary> Children { get; set; }
    }
}