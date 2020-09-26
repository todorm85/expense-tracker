using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Routing;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public abstract class GridBase : PageModel
    {
        protected readonly ITransactionsService transactionsService;
        protected string pageName;
        protected int initialMonthsBack = 0;
        protected readonly CategoriesService categories;

        public GridBase(ITransactionsService transactionsService, CategoriesService categories)
        {
            this.transactionsService = transactionsService;
            this.categories = categories;
        }

        [BindProperty]
        public IList<Transaction> Transactions { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime DateTo { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; }

        public List<SelectListItem> Categories { get; set; }

        public void OnGet()
        {
            InitializeDateFilters();
            Initialize();
            InitializeCategories();
        }

        protected abstract void Initialize();

        private void InitializeCategories()
        {
            this.Categories = new List<SelectListItem>()
            {
                new SelectListItem("Select Category", ""),
                new SelectListItem("Uncategorised", "-")
            };

            this.Categories = this.Categories.Union(
                this.Transactions
                    .Where(x => !string.IsNullOrEmpty(x.Category))
                    .Select(x => x.Category)
                    .OrderBy(x => x)
                    .Distinct()
                    .Select(x => new SelectListItem() { Text = x, Value = x })).ToList();
        }

        public IActionResult OnPostUpdate(int id)
        {
            var viewModel = Transactions.First(x => x.Id == id);
            var dbModel = this.transactionsService.GetAll(x => x.Id == id).First();
            if (viewModel.Category != dbModel.Category && viewModel.Category?.Contains(":") == true)
            {
                var parts = viewModel.Category.Split(":");
                viewModel.Category = parts[0];
                var key = parts[1];
                this.categories.Add(new Category[] { new Category() { Name = parts[0], KeyWord = parts[1] } });
                this.ClassifyAll();
            }

            dbModel.Details = viewModel.Details;
            dbModel.Amount = viewModel.Amount;
            dbModel.Date = viewModel.Date;
            dbModel.Category = viewModel.Category ?? "";
            this.transactionsService.Update(new Transaction[] { dbModel });
            return RedirectToPageWithState();
        }

        public IActionResult OnPostDelete(int id)
        {
            this.transactionsService.Remove(this.transactionsService.GetAll(x => x.Id == id));
            return RedirectToPageWithState();
        }

        public IActionResult OnPostSort()
        {
            return RedirectToPageWithState();
        }

        protected virtual RouteValueDictionary GetQueryParameters()
        {
            var queryParameters = new RouteValueDictionary();
            queryParameters.Add("DateFrom", DateFrom);
            queryParameters.Add("DateTo", DateTo);
            queryParameters.Add("XPosition", this.Request.Query["XPosition"]);
            queryParameters.Add("YPosition", this.Request.Query["YPosition"]);
            queryParameters.Add("CategoryFilter", CategoryFilter);
            return queryParameters;
        }

        protected IActionResult RedirectToPageWithState()
        {
            return RedirectToPage(this.pageName, this.GetQueryParameters());
        }

        protected IEnumerable<Transaction> ApplyCategoriesFilter(IEnumerable<Transaction> transactions)
        {
            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                if (CategoryFilter == "-")
                {
                    transactions = transactions.Where(x => string.IsNullOrWhiteSpace(x.Category));
                }
                else
                {
                    transactions = transactions.Where(x => x.Category != null && x.Category == CategoryFilter);
                }
            }

            return transactions;
        }

        protected IEnumerable<Transaction> GetTransactionsFilteredByDates()
        {
            return transactionsService
                            .GetAll(x => x.Date >= DateFrom &&
                                x.Date <= DateTo && !x.Ignored);
        }

        private void InitializeDateFilters()
        {
            var now = DateTime.Now;
            if (DateTo == default)
            {
                this.DateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            }

            if (DateFrom == default)
            {
                this.DateFrom = DateTime.Now.AddMonths(initialMonthsBack).SetToBeginningOfMonth();
            }
        }
        
        protected void ClassifyAll()
        {
            var all = this.transactionsService.GetAll().ToList();
            new TransactionsClassifier().Classify(all, this.categories.GetAll());
            this.transactionsService.Update(all);
        }
    }
}
