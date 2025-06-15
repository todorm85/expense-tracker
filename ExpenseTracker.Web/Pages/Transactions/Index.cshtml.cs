using ExpenseTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using ExpenseTracker.Core.Services;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly IExpensesService transactionsService;
        private readonly TransactionsFilterService transactionsFilterService;

        public IndexModel(IExpensesService transactions)
        {
            transactionsService = transactions;
            transactionsFilterService = new TransactionsFilterService(transactions);
        }

        [BindProperty]
        public TransactionsFilterViewModel Filter { get; set; } = new TransactionsFilterViewModel() {};

        [BindProperty]
        public TransactionsListModel TransactionsList { get; set; } = new TransactionsListModel() { DetailsHeight = 1 };

        [BindProperty]
        public PagerModel Pager { get; set; } = new PagerModel();

        public decimal Expenses { get; set; }

        public decimal Income { get; set; }

        public decimal Saved { get; set; }

        public void OnGet()
        {
            ModelState.Clear();
            var filterRes = transactionsFilterService.GetFilteredTransactions(Filter.ToFilterParams(this.Pager.PageIndex, this.Pager.PageSize));
            TransactionsList.Transactions = filterRes.Items.Select(t => new TransactionModel(t)).ToList();
            Filter.Apply(filterRes);
            this.Pager.PageCount = filterRes.PagesCount;
            this.Pager.PageIndex = filterRes.PageIndex;
        }

        public IActionResult OnPostFilterTransactions()
        {
            this.OnGet();
            return Page();
        }

        public IActionResult OnPostDeleteAll()
        {
            foreach (var t in this.transactionsFilterService.GetFilteredTransactions(Filter.ToFilterParams()).Items)
            {
                transactionsService.RemoveTransaction(t.TransactionId);
            }

            OnGet();
            return Page();
        }

        public IActionResult OnPostDeleteTransaction(string id)
        {
            transactionsService.RemoveTransaction(id);
            OnGet();
            return Page();
        }

        public IActionResult OnPostUpdateTransaction(string id)
        {
            var updated = TransactionsList.Transactions.First(x => x.TransactionId == id);
            transactionsService.UpdateTransaction(updated);
            OnGet();
            return Page();
        }

        public IActionResult OnPostUpdateAll()
        {
            foreach (var t in TransactionsList.Transactions)
            {
                transactionsService.UpdateTransaction(t);
            }

            OnGet();
            return Page();
        }

        public IActionResult OnPostNextPage()
        {
            this.Pager.PageIndex++;
            OnGet();
            return Page();
        }

        public IActionResult OnPostPreviousPage()
        {
            this.Pager.PageIndex--;
            OnGet();
            return Page();
        }

        public IActionResult OnPostProcessUncategorized()
        {
            transactionsService.ProcessAllUncategorizedTransactions();
            OnGet();
            return Page();
        }
    }
}