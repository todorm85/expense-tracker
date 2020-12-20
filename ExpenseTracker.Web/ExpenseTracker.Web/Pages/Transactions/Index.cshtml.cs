using ExpenseTracker.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class IndexModel : GridBase
    {
        public IndexModel(ITransactionsService transactions, CategoriesService categories)
            : base(transactions, categories)
        {
        }

        public decimal Expenses { get; set; }
        public decimal Income { get; set; }
        public decimal Saved { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Operation { get; set; }

        protected override void InitializeTransactions()
        {
            RefreshTransactions();
        }

        public IActionResult OnPostClassifyCurrent()
        {
            ClassifyFiltered();
            return RedirectToPageWithState();
        }

        public IActionResult OnPostDeleteFiltered()
        {
            var all = new List<Transaction>();
            foreach (var t in Transactions)
            {
                var tdb = this.transactionsService.GetAll(x => x.Id == t.Id).FirstOrDefault();
                if (tdb == null)
                    continue;
                all.Add(tdb);
            }

            this.transactionsService.Remove(all);
            return RedirectToPageWithState();
        }

        public IActionResult OnPostDeleteAll()
        {
            var all = this.transactionsService.GetAll().ToList();
            this.transactionsService.Remove(all);
            return RedirectToPageWithState();
        }

        public IActionResult OnPostClassifyAll()
        {
            ClassifyAll();
            return RedirectToPageWithState();
        }

        protected override RouteValueDictionary GetQueryParameters()
        {
            var parameters = base.GetQueryParameters();
            parameters.Add("Operation", this.Request.Query["Operation"]);
            return parameters;
        }

        private void ClassifyFiltered()
        {
            new TransactionsClassifier().Classify(this.Transactions, this.categories.GetAll());
            var all = new List<Transaction>();
            foreach (var t in Transactions)
            {
                var tdb = this.transactionsService.GetAll(x => x.Id == t.Id && string.IsNullOrEmpty(x.Category)).FirstOrDefault();
                if (tdb == null)
                    continue;
                tdb.Category = t.Category;
                all.Add(tdb);
            }

            this.transactionsService.Update(all);
        }

        private void RefreshTransactions()
        {
            IEnumerable<Transaction> transactions = GetTransactionsFiltered();

            IEnumerable<Transaction> sorted = new List<Transaction>();
            switch (this.Filters.SortBy)
            {
                case SortOptions.Date:
                    sorted = transactions.OrderByDescending(x => x.Date);
                    break;
                case SortOptions.Category:
                    sorted = transactions.OrderBy(x => x.Category);
                    break;
                case SortOptions.Amount:
                    sorted = transactions.OrderByDescending(x => x.Amount);
                    break;
                default:
                    break;
            }

            this.Transactions = sorted.ToList();
            this.Expenses = this.Transactions.Where(x => x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);
            this.Income = this.Transactions.Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount);
            this.Saved = this.Income - this.Expenses;
        }
    }
}
