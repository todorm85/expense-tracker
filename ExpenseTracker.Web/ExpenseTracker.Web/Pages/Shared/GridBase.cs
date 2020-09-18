using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Routing;

namespace ExpenseTracker.Web.Pages.Shared
{
    public class GridBase : PageModel
    {
        protected readonly ITransactionsService transactionsService;
        protected string pageName;
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
        public int XPosition { get; set; }

        [BindProperty(SupportsGet = true)]
        public int YPosition { get; set; }

        public virtual void OnGet()
        {
            InitializeDateFilters();
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
            return queryParameters;
        }

        protected IActionResult RedirectToPageWithState()
        {
            return RedirectToPage(this.pageName, this.GetQueryParameters());
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
                this.DateFrom = DateTime.Now.SetToBeginningOfMonth();
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
