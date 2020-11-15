using ExpenseTracker.Core;
using ExpenseTracker.Web.Models.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Models.Transactions
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
        public FiltersModel Filters { get; set; }

        public void OnGet()
        {
            InitializeFilters();
            InitializeTransactions();
            InitializeCategories();
        }

        protected abstract void InitializeTransactions();

        private void InitializeCategories()
        {
            this.Filters.Categories = new List<SelectListItem>()
            {
                new SelectListItem("Select Category", ""),
                new SelectListItem("Uncategorised", "-")
            };

            this.Filters.Categories = this.Filters.Categories.Union(
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
            queryParameters.Add("DateFrom", Filters.DateFrom);
            queryParameters.Add("DateTo", Filters.DateTo);
            queryParameters.Add("SortBy", Filters.SortBy);
            queryParameters.Add("Search", Filters.Search);
            AutoScrollScriptPartial.AppendQueryParamsFromRequest(this.Request, queryParameters);
            queryParameters.Add("CategoryFilter", Filters.CategoryFilter);
            return queryParameters;
        }

        protected IActionResult RedirectToPageWithState()
        {
            return RedirectToPage(this.pageName, this.GetQueryParameters());
        }

        protected IEnumerable<Transaction> GetTransactionsFiltered()
        {
            return transactionsService
                            .GetAll(x => ApplyDateFilter(x) &&
                                ApplyCategoriesFilter(x) &&
                                ApplySearchFilter(x) &&
                                !x.Ignored);
        }

        private bool ApplySearchFilter(Transaction x)
        {
            if (!string.IsNullOrWhiteSpace(Filters.Search))
            {
                return x.Details.Contains(Filters.Search);
            }

            return true;
        }

        private bool ApplyCategoriesFilter(Transaction x)
        {
            if (!string.IsNullOrWhiteSpace(Filters.CategoryFilter))
            {
                if (Filters.CategoryFilter == "-")
                {
                    return string.IsNullOrWhiteSpace(x.Category);
                }
                else
                {
                    return x.Category == Filters.CategoryFilter;
                }
            }

            return true;
        }

        private bool ApplyDateFilter(Transaction x)
        {
            return x.Date >= Filters.DateFrom && x.Date <= Filters.DateTo;
        }

        protected virtual void InitializeFilters()
        {
            var now = DateTime.Now;
            if (Filters.DateTo == default)
            {
                this.Filters.DateTo = new DateTime(now.Year, now.Month, now.Day, 23, 59, 59);
            }

            if (Filters.DateFrom == default)
            {
                this.Filters.DateFrom = DateTime.Now.AddMonths(initialMonthsBack).SetToBeginningOfMonth();
            }
        }

        protected void ClassifyAll()
        {
            var all = this.transactionsService.GetAll().ToList();
            new TransactionsClassifier().Classify(all, this.categories.GetAll());
            this.transactionsService.Update(all);
        }
    }

    public enum SortOptions
    {
        Date,
        Category,
        Amount
    }
}
