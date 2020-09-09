using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Logging;

namespace ExpenseTracker.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITransactionsService service;
        private readonly IBudgetCalculator calc;

        public IndexModel(ITransactionsService service, IBudgetCalculator calc)
        {
            this.service = service;
            this.calc = calc;
        }

        [BindProperty]
        public IList<Transaction> Transactions { get; set; }
        public decimal Expenses { get; private set; }
        public decimal Income { get; private set; }
        public decimal Saved { get; private set; }
        [BindProperty]
        public Transaction CreateTransaction { get; set; }

        [BindProperty(SupportsGet =true)]
        public DateTime DateFrom { get; set; }

        [BindProperty(SupportsGet =true)]
        public DateTime DateTo { get; set; }

        [BindProperty(SupportsGet =true)]
        public SortOptions SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; }

        public void OnGet()
        {
            if (DateFrom == default)
            {
                this.DateFrom = DateTime.Now.SetToBeginningOfMonth();
                this.DateTo = DateTime.Now;
            }

            RefreshTransactions();
        }

        public IActionResult OnPostDelete(int id)
        {
            this.service.Remove(this.service.GetAll(x => x.Id == id));
            return RedirectToPage();
        }

        public IActionResult OnPostUpdate(int id)
        {
            var viewModel = Transactions.First(x => x.Id == id);
            var dbModel = this.service.GetAll(x => x.Id == id).First();
            dbModel.Details = viewModel.Details;
            dbModel.Amount = viewModel.Amount;
            dbModel.Date = viewModel.Date;
            dbModel.Category = viewModel.Category;
            this.service.Update(new Transaction[] { dbModel });
            return RedirectToPage();
        }

        public IActionResult OnPostCreate(int expense)
        {
            var dbModel = new Transaction()
            {
                Amount = CreateTransaction.Amount,
                Category = CreateTransaction.Category,
                Date = CreateTransaction.Date,
                Details = CreateTransaction.Details,
                Type = (TransactionType)expense
            };
            this.service.Add(new Transaction[] { dbModel });
            return RedirectToPage();
        }

        public IActionResult OnPostSort()
        {
            return RedirectToPage("Index", 
                new 
                { 
                    SortBy,
                    DateFrom,
                    DateTo,
                    Search,
                    CategoryFilter
                });
        }

        private void RefreshTransactions()
        {
            var transactions = service
                .GetAll(x => x.Date >= DateFrom &&
                    x.Date <= DateTo);

            if (!string.IsNullOrWhiteSpace(Search))
            {
                transactions = transactions.Where(x => x.Details.Contains(Search));
            }

            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                transactions = transactions.Where(x => x.Category != null && x.Category.Contains(CategoryFilter));
            }

            IEnumerable<Transaction> sort = new List<Transaction>();
            switch (this.SortBy)
            {
                case SortOptions.Date:
                    sort = transactions.OrderByDescending(x => x.Date);
                    break;
                case SortOptions.Category:
                    sort = transactions.OrderBy(x => x.Category);
                    break;
                case SortOptions.Amount:
                    sort = transactions.OrderByDescending(x => x.Amount);
                    break;
                default:
                    break;
            }

            this.Transactions = sort.ToList();
            this.Expenses = this.Transactions.Where(x => x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);
            this.Income = this.Transactions.Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount);
            this.Saved = this.Income - this.Expenses;
        }
    }

    public enum SortOptions
    {
        Date,
        Category,
        Amount
    }
}
