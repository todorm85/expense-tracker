using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Rules;
using ExpenseTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.Json.Serialization;
using ExpenseTracker.Core.Services;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly IExpensesService transactionsService;

        public IndexModel(IExpensesService transactions)
        {
            transactionsService = transactions;
            TransactionsList = new TransactionsListModel()
            {
                DetailsHeight = 2
            };
        }

        [BindProperty]
        public TransactionsFilterViewModel Filter { get; set; }

        [BindProperty]
        public TransactionsListModel TransactionsList { get; set; }

        [BindProperty(SupportsGet = true)]
        public PagerModel Pager { get; set; }

        public decimal Expenses { get; set; }

        public decimal Income { get; set; }

        public decimal Saved { get; set; }

        public void OnGet(string filter)
        {
            LoadFilter(filter);
            LoadTransactions();
        }

        public IActionResult OnPostDeleteAll()
        {
            foreach (var t in this.transactionsService.GetAll(Filter.GetFilterQuery()))
            {
                transactionsService.RemoveTransaction(t.TransactionId);
            }

            return OnPost();
        }

        public IActionResult OnPostDeleteTransaction(string id)
        {
            transactionsService.RemoveTransaction(id);
            return OnPost();
        }

        public IActionResult OnPostUpdateTransaction(string id)
        {
            var updated = TransactionsList.Transactions.First(x => x.TransactionId == id);
            transactionsService.UpdateTransaction(updated);
            return OnPost();
        }

        public IActionResult OnPost()
        {
            return RedirectToPage(new { Filter, Pager.CurrentPage, Pager.PageSize });
        }

        public IActionResult OnPostUpdateAll()
        {
            foreach (var t in TransactionsList.Transactions)
            {
                transactionsService.UpdateTransaction(t);
            }

            return OnPost();
        }

        public IActionResult OnPostProcessUncategorized()
        {
            transactionsService.ProcessAllUncategorizedTransactions();
            return OnPost();
        }

        private void LoadFilter(string filter)
        {
            Filter = TransactionsFilterViewModel.FromString(filter, transactionsService);
            Pager.RouteParams.Add("filter", Filter.ToString());
        }

        private void LoadTransactions()
        {
            var filterQuery = Filter.GetFilterQuery();
            var sortBy = Filter.SortBy == SortOptions.None ? null : Filter.SortBy.ToString();
            var transactions = new PaginatedList<Transaction>(transactionsService, filterQuery, Pager.CurrentPage, Pager.PageSize, sortBy);
            TransactionsList.Transactions = transactions.Select(t => new TransactionModel(t)).ToList();
            Pager.PageCount = transactions.TotalPagesCount;
        }
    }

    public enum SortOptions
    {
        None,
        Date,
        Category,
        Amount
    }
}