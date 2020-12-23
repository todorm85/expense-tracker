using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Web.Pages.Budget
{
    public class Index : PageModel
    {
        private readonly IBudgetService service;

        public Index(IBudgetService service)
        {
            this.service = service;
        }

        public IEnumerable<Core.Budget> Budgets { get; private set; }

        public void OnGet()
        {
            this.Budgets = this.service.GetAll(x => x.ToMonth >= DateTime.Now.ToMonthStart())
                .OrderByDescending(b => b.ToMonth).ThenByDescending(b => b.FromMonth);
        }

        public IActionResult OnPostDelete(int id)
        {
            this.service.Remove(id);
            return this.RedirectToPage();
        }
    }
}
