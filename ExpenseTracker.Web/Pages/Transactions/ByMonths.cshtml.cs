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
        private readonly TransactionsFilterService transactionsFilter;

        public ByMonthsModel(IExpensesService expenses, TransactionsFilterService transactionsFilter)
        {
            this.expenses = expenses;
            this.transactionsFilter = transactionsFilter;
            this.Filters = new TransactionsFilterViewModel() { HideSorting = true };
        }

        public TransactionsFilterViewModel Filters { get; set; }

        public decimal AverageBalance => this.AverageIncome - this.AverageExpense;
        public decimal AverageExpense { get; private set; }
        public decimal AverageIncome { get; private set; }
        public decimal TotalExpense { get; private set; }
        public decimal TotalIncome { get; private set; }
        public decimal Balance => this.TotalIncome - this.TotalExpense;

        [BindProperty]
        public IEnumerable<IGrouping<(int Year, int Month), Transaction>> Months { get; private set; } = Enumerable.Empty<IGrouping<(int Year, int Month), Transaction>>();

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
            var all = this.transactionsFilter.GetFilteredItems(this.Filters.ToFilterParams()).Items;
            if (all.Count() == 0)
                return;

            this.Months = all
                .GroupBy(x => (x.Date.Year, x.Date.Month))
                .OrderByDescending(x => x.Key.Year)
                .ThenByDescending(x => x.Key.Month);

            // Calculate total income and expense
            this.TotalIncome = all.Where(x => x.Type == TransactionType.Income).Sum(x => x.Amount);
            this.TotalExpense = all.Where(x => x.Type == TransactionType.Expense).Sum(x => Math.Abs(x.Amount));

            // Calculate averages per month (only for months that have transactions)
            int monthCount = this.Months.Count();
            this.AverageIncome = monthCount > 0 ? this.TotalIncome / monthCount : 0;
            this.AverageExpense = monthCount > 0 ? this.TotalExpense / monthCount : 0;
        }
    }
}
