using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Web.Pages
{
    public class TransactionsByMonthByCategoryModel : PageModel
    {
        private readonly ITransactionsService transactionsService;

        public TransactionsByMonthByCategoryModel(
            ITransactionsService transactionsService)
        {
            this.transactionsService = transactionsService;
        }

        [BindProperty]
        public IList<Transaction> Transactions { get; set; }

        public IEnumerable<string> ExpandedElements { get; set; }

        public IDictionary<DateTime, IDictionary<string, decimal>> MonthsCategoriesTotals { get;  set; }

        public IDictionary<DateTime, decimal> MonthsTotals { get; private set; }

        public void OnGet()
        {
            this.MonthsCategoriesTotals = new Dictionary<DateTime, IDictionary<string, decimal>>();
            this.MonthsTotals = new Dictionary<DateTime, decimal>();
            this.Transactions = new List<Transaction>();

            var months = this.transactionsService.GetAll(x => x.Date >= DateTime.Now.AddMonths(-6) && x.Date <= DateTime.Now && x.Type == TransactionType.Expense)
                .ToLookup(x => x.Date.SetToBeginningOfMonth()).OrderByDescending(x => x.Key);
            foreach (var month in months)
            {
                this.MonthsTotals[month.Key] = month.Sum(x => x.Amount);
                this.MonthsCategoriesTotals[month.Key] = new Dictionary<string, decimal>();
                var categories = month.ToLookup(x => x.Category).OrderByDescending(x => x.Sum(y => y.Amount));
                foreach (var c in categories)
                {
                    this.MonthsCategoriesTotals[month.Key][c.Key ?? ""] = c.Sum(x => x.Amount);
                    foreach (var t in c)
                    {
                        this.Transactions.Add(t);
                    }
                }
            }

            this.ExpandedElements = new List<string>();
        }

        public IActionResult OnPostUpdateTransaction()
        {
            return RedirectToPage();
        }
    }
}
