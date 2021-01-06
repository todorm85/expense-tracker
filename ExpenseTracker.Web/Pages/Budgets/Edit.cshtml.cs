using ExpenseTracker.Core;
using ExpenseTracker.Core.Budget;
using ExpenseTracker.Core.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Budgets
{
    public class EditModel : PageModel
    {
        private readonly IBudgetService service;

        public EditModel(IBudgetService service)
        {
            this.service = service;
        }

        [BindProperty]
        public Core.Budget.Budget Budget { get; set; }

        [BindProperty]
        public Transaction TransactionToAdd { get; set; }

        public TransactionType TrType { get; set; }

        public void OnGet(int id)
        {
            if (id != default)
            {
                this.Budget = this.service.GetAll(x => x.Id == id).First();
            }
            else
            {
                this.Budget = new Core.Budget.Budget()
                {
                    FromMonth = DateTime.Now.ToMonthStart(),
                    ToMonth = DateTime.Now.AddMonths(1).ToMonthEnd()
                };
            }
        }

        public void OnPostAddTransaction()
        {
            this.Budget.ExpectedTransactions.Add(TransactionToAdd);
            this.TransactionToAdd = new Transaction();
        }

        public void OnPostRemoveTransaction(int idx)
        {
            this.ModelState.Clear();
            this.Budget.ExpectedTransactions.RemoveAt(idx);
        }

        public IActionResult OnPostSave()
        {
            if (this.Budget.Id != default)
            {
                this.service.Update(this.Budget);
            }
            else
            {
                this.service.Insert(this.Budget);
            }

            return RedirectToPage("Index");
        }
    }
}