using ExpenseTracker.Core;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Transactions.Rules;
using ExpenseTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class YearMonthCategoryModel : PageModel
    {
        private readonly ITransactionsService transactionsService;
        private readonly IGenericRepository<Rule> rules;

        public YearMonthCategoryModel(
            ITransactionsService transactionsService, IGenericRepository<Rule> rules)
        {
            this.CategorySummaries = new List<CategorySummary>();
            this.ExpandableMonths = new List<ExpandableMonthModel>();
            this.transactionsService = transactionsService;
            this.rules = rules;
            this.Filters = new TransactionsFilterViewModel() { HideSorting = true };
        }

        public IList<CategorySummary> CategorySummaries { get; set; }
        public decimal AverageBalance => this.AverageIncome - this.AverageExpense;
        public decimal AverageExpense { get; private set; }
        public decimal AverageIncome { get; private set; }
        public decimal Balance => this.TotalIncome - this.TotalExpense;
        public List<ExpandableMonthModel> ExpandableMonths { get; set; }

        [BindProperty]
        public TransactionsFilterViewModel Filters { get; set; }

        [BindProperty]
        public TransactionModel UpdatedTransaction { get; set; }

        public decimal TotalExpense { get; private set; }

        public decimal TotalIncome { get; private set; }

        [BindProperty(SupportsGet = true)]
        public string ToggledElements { get; set; }

        public void OnGet(string filters)
        {
            this.Filters = TransactionsFilterViewModel.FromString(filters, transactionsService);
            this.Filters.HideSorting = true;
            var all = this.transactionsService.GetAll(this.Filters.GetFilterQuery());
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

        public IActionResult OnPostUpdateTransaction()
        {
            UpdatedTransaction.Update(transactionsService, rules);
            return OnPost(true);
        }

        public IActionResult OnPostDeleteTransaction(string id)
        {
            transactionsService.RemoveById(id);
            return OnPost(true);
        }

        private static void GetAllCategorySummaries(List<CategorySummary> catSums, IEnumerable<ExpandableCategoryModel> categories)
        {
            while (categories.Count() > 0)
            {
                var nextBatch = new List<ExpandableCategoryModel>();
                foreach (var expandableCategory in categories)
                {
                    IList<CategorySummary> currentLevelCategorySummaries = catSums;
                    var path = ExpandableMonthModel.GetCategoryKey(expandableCategory.CategoryName).Split('/');
                    for (int i = 0; i < path.Length; i++)
                    {
                        var cs = currentLevelCategorySummaries.FirstOrDefault(x => x.Name == path[i]);
                        if (cs == null)
                        {
                            cs = new CategorySummary() { Name = path[i] };
                            currentLevelCategorySummaries.Add(cs);
                        }

                        cs.Totals += expandableCategory.Balance;
                        currentLevelCategorySummaries = cs.Children;
                    }

                    if (expandableCategory.Categories.Count > 0)
                    {
                        nextBatch.AddRange(expandableCategory.Categories);
                    }
                }

                categories = nextBatch;
            }
        }

        public IActionResult OnPost(bool keepToggledSections = false)
        {
            if (keepToggledSections)
            {
                return RedirectToPage(new { Filters, ToggledElements });
            }
            else
            {
                return RedirectToPage(new { Filters });
            }
        }
    }

    public class CategorySummary
    {
        public CategorySummary()
        {
            this.Children = new List<CategorySummary>();
            this.Level = 1;
        }

        public string Name { get; set; }
        public decimal Average { get; set; }
        public decimal Totals { get; set; }
        public IList<CategorySummary> Children { get; set; }
        public int Level { get; set; }
    }
}