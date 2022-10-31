using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Unity;

namespace ExpenseTracker.Web.Pages.Shared.Components.TransactionsSummary
{
    public class TransactionsSummaryViewComponent : ViewComponent
    {
        private readonly IExpensesService service;

        public TransactionsSummaryViewComponent(IExpensesService service)
        {
            this.service = service;
        }

        public decimal Expenses { get; set; }
        public decimal Income { get; set; }
        public decimal Saved { get; set; }

        public IViewComponentResult Invoke(TransactionsFilterViewModel filterModel)
        {
            var all = service.GetAll(filterModel.GetFilterQuery());
            this.Expenses = all.Where(x => x.Type == TransactionType.Expense)
                            .Sum(x => x.Amount);
            this.Income = all.Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount);
            this.Saved = this.Income - this.Expenses;

            return View(this);
        }
    }
}
