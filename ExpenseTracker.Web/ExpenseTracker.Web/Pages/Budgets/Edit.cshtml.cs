using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Web.Pages.Budgets
{
    public class EditModel : PageModel
    {
        private readonly IBudgetService service;

        public EditModel(IBudgetService service)
        {
            this.service = service;
        }

        public TransactionType TrType { get; set; }
        
        [BindProperty]
        public Core.Budget Budget { get; set; }

        [BindProperty]
        public Transaction TransactionToAdd { get; set; }

        public void OnGet(int id)
        {
            if (id != default)
            {
                this.Budget = this.service.GetAll(x => x.Id == id).First();
            }
            else
            {
                this.Budget = new Core.Budget()
                {
                    FromMonth = DateTime.Now.ToMonthStart(),
                    ToMonth = DateTime.Now.AddMonths(1).ToMonthEnd()
                };
            }
        }

        public IActionResult OnPostAddTransaction()
        {
            this.Budget.ExpectedTransactions.Add(TransactionToAdd);
            this.TransactionToAdd = new Transaction();
            return this.Page();
        }

        public IActionResult OnPostRemoveTransaction(int idx)
        {
            this.Budget.ExpectedTransactions.RemoveAt(idx);
            return this.Page();
        }

        public IActionResult OnPostSave()
        {
            if (this.Budget.Id != default)
            {
                this.service.Update(this.Budget);
            }
            else
            {
                this.service.Add(this.Budget);
            }

            return RedirectToPage("Index");
        }
    }
}
