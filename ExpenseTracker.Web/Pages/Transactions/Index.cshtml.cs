using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Transactions.Rules;
using ExpenseTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using System.Collections.Generic;
using System;
using System.Linq;
using System.Text.Json.Serialization;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly ITransactionsService transactionsService;
        private readonly IGenericRepository<Rule> rules;

        public IndexModel(ITransactionsService transactions, IGenericRepository<Rule> rules)
        {
            transactionsService = transactions;
            this.rules = rules;
            TransactionsList = new TransactionsListModel();
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
            foreach (var t in TransactionsList.Transactions)
            {
                transactionsService.RemoveById(t.TransactionId);
            }

            return OnPost();
        }

        public IActionResult OnPostDeleteTransaction(string id)
        {
            transactionsService.RemoveById(id);
            return OnPost();
        }

        public IActionResult OnPostUpdateTransaction(string id)
        {
            var updated = TransactionsList.Transactions.First(x => x.TransactionId == id);
            updated.Update(transactionsService, rules);
            return OnPost();
        }

        public IActionResult OnPost()
        {
            return RedirectToPage(new { Filter, Pager.CurrentPage, Pager.PageSize });
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