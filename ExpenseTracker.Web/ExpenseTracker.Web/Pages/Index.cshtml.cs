using System;
using System.Collections.Generic;
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

        public IndexModel(ITransactionsService service)
        {
            this.service = service;
        }

        [BindProperty]
        public IList<Transaction> Transactions { get; set; }

        [BindProperty]
        public Transaction CreateTransaction { get; set; }

        public void OnGet()
        {
            this.Transactions = service.GetAll().Where(x => x.Date >= DateTime.Now.AddMonths(-1)).OrderByDescending(x => x.Date).ToList();
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

    }
}
