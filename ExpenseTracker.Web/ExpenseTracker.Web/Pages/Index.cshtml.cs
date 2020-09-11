using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Allianz;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.Extensions.Logging;
using Org.BouncyCastle.Asn1.X509.Qualified;
using Org.BouncyCastle.Crypto.Digests;

namespace ExpenseTracker.Web.Pages
{
    public class IndexModel : PageModel
    {
        private readonly ITransactionsService service;
        private readonly CategoriesService categories;
        private readonly AllianzTxtFileParser allianz;
        private readonly RaiffeizenTxtFileParser rai;

        public IndexModel(ITransactionsService expenses, CategoriesService categories, AllianzTxtFileParser allianz, RaiffeizenTxtFileParser rai)
        {
            this.service = expenses;
            this.categories = categories;
            this.allianz = allianz;
            this.rai = rai;
        }

        [BindProperty]
        public IList<Transaction> Transactions { get; set; }
        public decimal Expenses { get; set; }
        public decimal Income { get; set; }
        public decimal Saved { get; set; }
        [BindProperty]
        public Transaction CreateTransaction { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime DateFrom { get; set; }

        [BindProperty(SupportsGet = true)]
        public DateTime DateTo { get; set; }

        [BindProperty(SupportsGet = true)]
        public SortOptions SortBy { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Search { get; set; }

        [BindProperty(SupportsGet = true)]
        public string CategoryFilter { get; set; }

        public List<SelectListItem> Categories { get; set; }

        [BindProperty]
        public IList<IFormFile> Files { get; set; }

        public void OnGet()
        {
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
            if (DateFrom == default)
            {
                this.DateFrom = DateTime.Now.SetToBeginningOfMonth();
                this.DateTo = DateTime.Now;
            }

            RefreshTransactions();
        }

        public IActionResult OnPostDelete(int id)
        {
            this.service.Remove(this.service.GetAll(x => x.Id == id));
            return RedirectToPageWithState();
        }

        public IActionResult OnPostUpdate(int id)
        {
            var viewModel = Transactions.First(x => x.Id == id);
            var dbModel = this.service.GetAll(x => x.Id == id).First();
            if (viewModel.Category != dbModel.Category && viewModel.Category.Contains(":"))
            {
                var parts = viewModel.Category.Split(":");
                viewModel.Category = parts[0];
                var key = parts[1];
                this.categories.Add(new Category[] { new Category() { Name = parts[0], KeyWord = parts[1] } });
            }

            dbModel.Details = viewModel.Details;
            dbModel.Amount = viewModel.Amount;
            dbModel.Date = viewModel.Date;
            dbModel.Category = viewModel.Category;
            this.service.Update(new Transaction[] { dbModel });
            return RedirectToPageWithState();
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
            this.service.Add(new Transaction[] { dbModel });
            return RedirectToPageWithState();
        }

        public IActionResult OnPostSort()
        {
            return RedirectToPageWithState();
        }

        public IActionResult OnPostClassifyCurrent()
        {
            new TransactionsClassifier().Classify(this.Transactions, this.categories.GetAll());
            var all = new List<Transaction>();
            foreach (var t in Transactions)
            {
                var tdb = this.service.GetAll(x => x.Id == t.Id && string.IsNullOrEmpty(x.Category)).FirstOrDefault();
                if (tdb == null)
                    continue;
                tdb.Category = t.Category;
                all.Add(tdb);
            }

            this.service.Update(all);
            return RedirectToPageWithState();
        }

        public IActionResult OnPostDeleteFiltered()
        {
            var all = new List<Transaction>();
            foreach (var t in Transactions)
            {
                var tdb = this.service.GetAll(x => x.Id == t.Id).FirstOrDefault();
                if (tdb == null)
                    continue;
                all.Add(tdb);
            }

            this.service.Remove(all);
            return RedirectToPageWithState();
        }

        public IActionResult OnPostDeleteAll()
        {
            var all = this.service.GetAll().ToList();
            this.service.Remove(all);
            return RedirectToPageWithState();
        }

        public IActionResult OnPostClassifyAll()
        {
            var all = this.service.GetAll().ToList();
            new TransactionsClassifier().Classify(all, this.categories.GetAll());
            this.service.Update(all);
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
                        this.service.Add(expenses);
                    }
                    else if (formFile.FileName.EndsWith("txt"))
                    {
                        IEnumerable<Transaction> expenses = this.allianz.GetTransactions(filePath);
                        this.service.Add(expenses);
                    }
                }
            }

            return RedirectToPageWithState();
        }

        private void RefreshTransactions()
        {
            var transactions = service
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

        private IActionResult RedirectToPageWithState()
        {
            return RedirectToPage("Index",
                new
                {
                    SortBy,
                    DateFrom,
                    DateTo,
                    Search,
                    CategoryFilter
                });
        }
    }

    public enum SortOptions
    {
        Date,
        Category,
        Amount
    }
}
