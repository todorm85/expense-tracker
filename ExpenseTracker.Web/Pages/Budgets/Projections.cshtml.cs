using ExpenseTracker.Core;
using ExpenseTracker.Core.Budget;
using ExpenseTracker.Core.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Budgets
{
    public class ProjectionsModel : PageModel
    {
        private readonly IBudgetService budgetsService;

        public ProjectionsModel(IBudgetService budgetsService)
        {
            this.budgetsService = budgetsService;
            this.FromMonth = DateTime.Now.ToMonthStart();
            this.ToMonth = this.FromMonth.AddMonths(6);
            this.Projections = new List<MonthProjection>();
        }

        public decimal AllMonthsExpenses { get; set; }

        public decimal AllMonthsIncome { get; set; }

        [BindProperty]
        public DateTime FromMonth { get; set; }

        public IList<MonthProjection> Projections { get; set; }

        [BindProperty]
        public DateTime ToMonth { get; set; }

        public void OnGet()
        {
            var currentMonth = FromMonth.ToMonthStart();
            while (currentMonth <= ToMonth.ToMonthStart())
            {
                var budget = this.budgetsService.GetCumulativeForMonth(currentMonth);
                if (budget != null)
                {
                    var expenses = budget.ExpectedTransactions
                            .Where(x => x.Type == TransactionType.Expense);
                    var totalExpenses = 0m;
                    if (expenses.Count() > 0)
                    {
                        totalExpenses = expenses.Select(x => x.Amount).Aggregate((x, y) => { return x + y; });
                    }
                    var income = budget.ExpectedTransactions
                    .Where(x => x.Type == TransactionType.Income);
                    var totalIncome = 0m;
                    if (income.Count() > 0)
                    {
                        totalIncome = income.Select(x => x.Amount).Aggregate((x, y) => { return x + y; });
                    }

                    this.Projections.Add(new MonthProjection()
                    {
                        Month = currentMonth,
                        TotalExpense = totalExpenses,
                        TotalIncome = totalIncome,
                        ExpectedTransactions = budget.ExpectedTransactions
                    });
                }

                currentMonth = currentMonth.AddMonths(1);
            }

            this.AllMonthsExpenses = this.Projections.Select(x => x.TotalExpense).Aggregate((x, y) => x + y);
            this.AllMonthsIncome = this.Projections.Select(x => x.TotalIncome).Aggregate((x, y) => x + y);
        }

        public IActionResult OnPostFilter()
        {
            this.OnGet();
            return Page();
        }

        public class MonthProjection
        {
            public IEnumerable<Transaction> ExpectedTransactions { get; set; }
            public DateTime Month { get; set; }
            public decimal TotalExpense { get; set; }
            public decimal TotalIncome { get; set; }
        }
    }
}