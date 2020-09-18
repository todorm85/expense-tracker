using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Allianz;
using ExpenseTracker.Core;
using ExpenseTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Org.BouncyCastle.Crypto.Digests;

namespace ExpenseTracker.Web.Pages
{
    public class IndexModel : GridBase
    {
        private readonly AllianzTxtFileParser allianz;
        private readonly RaiffeizenTxtFileParser rai;

        public IndexModel(ITransactionsService transactions, CategoriesService categories, AllianzTxtFileParser allianz, RaiffeizenTxtFileParser rai)
            : base (transactions, categories)
        {
            this.pageName = "List";
            this.allianz = allianz;
            this.rai = rai;
        }

        public decimal Expenses { get; set; }
        public decimal Income { get; set; }
        public decimal Saved { get; set; }
        [BindProperty]
        public Transaction CreateTransaction { get; set; }

        [BindProperty(SupportsGet = true)]
        public SortOptions SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; }

        public List<SelectListItem> Categories { get; set; }

        [BindProperty]
        public IList<IFormFile> Files { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Operation { get; set; }

        public override void OnGet()
        {
            base.OnGet();
            this.Categories = new List<SelectListItem>()
            {
                new SelectListItem("Select Category", ""),
                new SelectListItem("Uncategorised", "-")
            };

            this.Categories = this.Categories.Union(
                this.categories.GetAll()
                    .Select(x => x.Name)
                    .OrderBy(x => x)
                    .Distinct()
                    .Select(x => new SelectListItem() { Text = x, Value = x })).ToList();

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

        public IActionResult OnPostUpload(List<IFormFile> files)
        {
            foreach (var formFile in files)
            {
                if (formFile.Length > 0)
                {
                    var filePath = Path.GetTempFileName();

                    using (var stream = System.IO.File.Create(filePath))
                    {
                        formFile.CopyTo(stream);
                    }

                    if (formFile.FileName.EndsWith("xml"))
                    {
                        IEnumerable<Transaction> expenses = this.rai.ParseFile(filePath);
                        this.transactionsService.Add(expenses);
                    }
                    else if (formFile.FileName.EndsWith("txt"))
                    {
                        IEnumerable<Transaction> expenses = this.allianz.GetTransactions(filePath);
                        this.transactionsService.Add(expenses);
                    }
                }
            }

            return RedirectToPageWithState();
        }

        protected override RouteValueDictionary GetQueryParameters()
        {
            var parameters = base.GetQueryParameters();
            parameters.Add("SortBy", SortBy);
            parameters.Add("Search", Search);
            parameters.Add("CategoryFilter", CategoryFilter);
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
            var transactions = transactionsService
                .GetAll(x => x.Date >= DateFrom &&
                    x.Date <= DateTo);

            if (!string.IsNullOrWhiteSpace(Search))
            {
                transactions = transactions.Where(x => x.Details.Contains(Search));
            }

            if (!string.IsNullOrWhiteSpace(CategoryFilter))
            {
                if (CategoryFilter == "-")
                {
                    transactions = transactions.Where(x => string.IsNullOrWhiteSpace(x.Category));
                }
                else
                {
                    transactions = transactions.Where(x => x.Category != null && x.Category.Contains(CategoryFilter));
                }
            }

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

    public enum SortOptions
    {
        Date,
        Category,
        Amount
    }
}
