using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.Allianz;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class IndexModel : GridBase
    {
        public IndexModel(ITransactionsService transactions, CategoriesService categories)
            : base (transactions, categories)
        {
            this.pageName = "List";
        }

        public decimal Expenses { get; set; }
        public decimal Income { get; set; }
        public decimal Saved { get; set; }
        [BindProperty]
        public Transaction CreateTransaction { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Operation { get; set; }

        protected override void InitializeTransactions()
        {
            this.CreateTransaction = new Transaction() { Date = DateTime.Now };
            RefreshTransactions();
        }

        public IActionResult OnPostCreate(int expense)
        {
            var dbModel = new Transaction()
            {
                Amount = CreateTransaction.Amount,
                Category = CreateTransaction.Category,
                Date = CreateTransaction.Date,
                Details = CreateTransaction.Details,
                Type = (TransactionType)expense
            };
            this.transactionsService.Add(new Transaction[] { dbModel });
            return RedirectToPageWithState();
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

            IEnumerable<Transaction> sort = new List<Transaction>();
            switch (this.SortBy)
            {
                case SortOptions.Date:
                    sort = transactions.OrderByDescending(x => x.Date);
                    break;
                case SortOptions.Category:
                    sort = transactions.OrderBy(x => x.Category);
                    break;
                case SortOptions.Amount:
                    sort = transactions.OrderByDescending(x => x.Amount);
                    break;
                default:
                    break;
            }

            this.Transactions = sort.ToList();
            this.Expenses = this.Transactions.Where(x => x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);
            this.Income = this.Transactions.Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount);
            this.Saved = this.Income - this.Expenses;
        }
    }
}
