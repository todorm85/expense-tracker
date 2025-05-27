using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Web.Pages.Shared.Components.TransactionsSummary
{
    public class TransactionsSummaryViewComponent : ViewComponent
    {
        private readonly TransactionsFilterService service;

        public TransactionsSummaryViewComponent(TransactionsFilterService service)
        {
            this.service = service;
        }

        public decimal Expenses { get; set; }
        public decimal Income { get; set; }
        public decimal Saved { get; set; }

        public IViewComponentResult Invoke(TransactionsFilterViewModel filterModel)
        {
            var all = service.GetFilteredItems(filterModel.ToFilterParams()).Items;
            this.Expenses = all.Where(x => x.Type == TransactionType.Expense)
                            .Sum(x => x.Amount);
            this.Income = all.Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount);
            this.Saved = this.Income - this.Expenses;

            return View(this);
        }
    }
}
