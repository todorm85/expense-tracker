using System;
using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class ByMonthsModel : PageModel
    {
        private readonly IExpensesService expenses;

        public ByMonthsModel(IExpensesService expenses)
        {
            this.expenses = expenses;
            this.Filters = new TransactionsFilterViewModel() { HideSorting = true };
        }

        [BindProperty]
        public TransactionsFilterViewModel Filters { get; set; }

        public decimal AverageBalance => this.AverageIncome - this.AverageExpense;
        public decimal AverageExpense { get; private set; }
        public decimal AverageIncome { get; private set; }
        public decimal TotalExpense { get; private set; }
        public decimal TotalIncome { get; private set; }
        public decimal Balance => this.TotalIncome - this.TotalExpense;

        [BindProperty]
        public IEnumerable<IGrouping<(int Year, int Month), Transaction>> Months { get; private set; }

        public void OnGet()
        {
            Init();
        }

        public void OnPost()
        {
            Init();
        }

        private void Init()
        {
            22var all = this.expenses.GetAll(this.Filters.GetFilterQuery());
            if (all.Count() == 0)
                return;

            this.Months = all
                .GroupBy(x => (x.Date.Year, x.Date.Month))
                .OrderByDescending(x => x.Key.Year)
                .ThenByDescending(x => x.Key.Month);
        }
    }
}
