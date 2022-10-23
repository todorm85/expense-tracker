﻿using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Transactions.Rules;
using ExpenseTracker.Web.Pages.Shared;
using ExpenseTracker.Web.Pages.Shared.Components.Filter;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public enum SortOptions
    {
        None,
        Date,
        Category,
        Amount
    }

    public class IndexModel : PageModel
    {
        private readonly ITransactionsService transactionsService;

        public IndexModel(ITransactionsService transactions)
        {
            transactionsService = transactions;
            TransactionsList = new TransactionsListModel();
        }

        [BindProperty]
        public FiltersViewModel Filter { get; set; }

        public TransactionsListModel TransactionsList { get; set; }
        
        [BindProperty(SupportsGet = true)]
        public PagerModel Pager { get; set; }

        public decimal Expenses { get; set; }

        public decimal Income { get; set; }

        public decimal Saved { get; set; }
        
        public void OnGet(string filter)
        {
            Filter = filter != null ? ModelSerialization.Deserialize<FiltersViewModel>(filter) : new FiltersViewModel();
            var filterQuery = Filter.GetFilterQuery();
            var sortBy = Filter.SortBy == SortOptions.None ? null : Filter.SortBy.ToString();
            var transactions = new PaginatedList<Transaction>(transactionsService, filterQuery, Pager.CurrentPage, 10, sortBy);
            TransactionsList.Transactions = transactions.Select(t => new TransactionModel(t)).ToList();
            
            Pager.PageCount = transactions.TotalPagesCount;
            Pager.RouteParams.Add("filter", Filter.ToString());

            Expenses = TransactionsList.Transactions.Where(x => x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);
            Income = TransactionsList.Transactions.Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount);
            Saved = Income - Expenses;
        }

        public IActionResult OnPostDeleteFiltered()
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

        public IActionResult OnPost()
        {
            return RedirectToPage(new { Filter });
        }
    }
}