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
        private readonly TransactionsFilterService transactionsFilterService;

        public IndexModel(IExpensesService transactions)
        {
            transactionsService = transactions;
            transactionsFilterService = new TransactionsFilterService(transactions);
            TransactionsList = new TransactionsListModel()
            {
                DetailsHeight = 2,
                //ShowSource = true,
            };
        }

        [BindProperty]
        public TransactionsFilterViewModel Filter { get; set; } = new TransactionsFilterViewModel();

        [BindProperty]
        public TransactionsListModel TransactionsList { get; set; }

        [BindProperty]
        public PagerModel Pager { get; set; } = new PagerModel() { PagePath = "./Index", PageSize = 20, CurrentPageIndex = 0 };

        public decimal Expenses { get; set; }

        public decimal Income { get; set; }

        public decimal Saved { get; set; }

        [BindProperty(SupportsGet =true)]
        public int PageIndex { get; set; }

        public void OnGet(TransactionsFilterViewModel filter, PagerModel pager)
        {
            this.Pager = TempData.Get<PagerModel>("pager") ?? this.Pager;
            this.Filter = TempData.Get<TransactionsFilterViewModel>("filter") ?? this.Filter;
            LoadTransactions();
        }

        public IActionResult OnPost()
        {
            TempData.Set("filter", Filter);
            TempData.Set("pager", Pager);
            return RedirectToPage();
        }

        public IActionResult OnPostDeleteAll()
        {
            foreach (var t in this.transactionsFilterService.GetFilteredTransactions(Filter.ToFilterParams()).Items)
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

        private void LoadTransactions()
        {
            var filterRes = transactionsFilterService.GetFilteredTransactions(Filter.ToFilterParams(this.PageIndex, this.Pager.PageSize));

            TransactionsList.Transactions = filterRes.Items.Select(t => new TransactionModel(t)).ToList();

            Filter.Apply(filterRes);

            this.Pager.CurrentPageIndex = this.PageIndex;
            this.Pager.PageCount = filterRes.PagesCount;
            
        }
    }
}