﻿using ExpenseTracker.Core.Categories;
using ExpenseTracker.Core.Transactions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public enum SortOptions
    {
        Date,
        Category,
        Amount
    }

    public class IndexModel : PageModel
    {
        private readonly CategoriesService categories;
        private readonly int initialMonthsBack = 0;
        private readonly ITransactionsService transactionsService;

        public IndexModel(ITransactionsService transactions, CategoriesService categories)
        {
            this.transactionsService = transactions;
            this.categories = categories;
            this.Filters = new FiltersModel(initialMonthsBack);
            this.TransactionsList = new TransactionsListModel();
        }

        public decimal Expenses { get; set; }

        [BindProperty]
        public FiltersModel Filters { get; set; }

        public decimal Income { get; set; }
        public decimal Saved { get; set; }

        [BindProperty]
        public TransactionsListModel TransactionsList { get; set; }

        public void OnGet()
        {
            this.ModelState.Clear();
            IEnumerable<Transaction> transactions = this.Filters.GetTransactionsFiltered(this.transactionsService);
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

            this.TransactionsList.Transactions = sorted.ToList();
            this.Expenses = this.TransactionsList.Transactions.Where(x => x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);
            this.Income = this.TransactionsList.Transactions.Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount);
            this.Saved = this.Income - this.Expenses;
        }

        public void OnPost()
        {
            this.OnGet();
        }

        public void OnPostClassifyCurrent()
        {
            new TransactionsClassifier().Classify(this.TransactionsList.Transactions, this.categories.GetAll());
            var all = new List<Transaction>();
            foreach (var t in TransactionsList.Transactions)
            {
                var tdb = this.transactionsService.GetAll(x => x.TransactionId == t.TransactionId && string.IsNullOrEmpty(x.Category)).FirstOrDefault();
                if (tdb == null)
                    continue;
                tdb.Category = t.Category;
                all.Add(tdb);
            }

            this.transactionsService.Update(all);
            this.OnGet();
        }

        public void OnPostDeleteAll()
        {
            this.transactionsService.RemoveById(this.transactionsService.GetAll());
            this.ModelState.Clear();
            this.TransactionsList.Transactions.Clear();
        }

        public void OnPostDeleteFiltered()
        {
            var all = new List<Transaction>();
            foreach (var t in TransactionsList.Transactions)
            {
                var tdb = this.transactionsService.GetAll(x => x.TransactionId == t.TransactionId).FirstOrDefault();
                if (tdb == null)
                    continue;
                all.Add(tdb);
            }

            this.transactionsService.RemoveById(all);
            this.OnGet();
        }
    }
}