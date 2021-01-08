using ExpenseTracker.Core;
using ExpenseTracker.Core.Budget;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Budget
{
    public class Index : PageModel
    {
        private readonly IBudgetService service;

        public Index(IBudgetService service)
        {
            this.service = service;
        }

        public IEnumerable<Core.Budget.Budget> Budgets { get; set; }

        public void OnGet()
        {
            this.Budgets = this.service.GetAll(x => x.ToMonth >= DateTime.Now.ToMonthStart())
                .OrderByDescending(b => b.ToMonth).ThenByDescending(b => b.FromMonth);
        }

        public IActionResult OnPostDelete(int id)
        {
            this.service.RemoveById(id);
            return this.RedirectToPage();
        }
    }
}