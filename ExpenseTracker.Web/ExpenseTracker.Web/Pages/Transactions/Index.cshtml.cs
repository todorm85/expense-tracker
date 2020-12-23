using ExpenseTracker.Core;
using ExpenseTracker.Web.Models.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text.Json;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class IndexModel : PageModel
    {
        private readonly ITransactionsService transactionsService;
        private readonly CategoriesService categories;
        private readonly int initialMonthsBack = 0;

        public IndexModel(ITransactionsService transactions, CategoriesService categories)
        {
            this.transactionsService = transactions;
            this.categories = categories;
            this.Filters = new FiltersModel(initialMonthsBack);
        }

        public decimal Expenses { get; set; }
        public decimal Income { get; set; }
        public decimal Saved { get; set; }
        [BindProperty]
        public IList<Transaction> Transactions { get; set; }
        [BindProperty]
        public FiltersModel Filters { get; set; }

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

            this.Transactions = sorted.ToList();
            this.Expenses = this.Transactions.Where(x => x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);
            this.Income = this.Transactions.Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount);
            this.Saved = this.Income - this.Expenses;
        }

        public void OnPost()
        {
            this.OnGet();
        }

        public IActionResult OnPostUpdate()
        {
            MemoryStream stream = new MemoryStream();
            Request.Body.CopyToAsync(stream);
            stream.Position = 0;
            Transaction viewModel = null;
            using (StreamReader reader = new StreamReader(stream))
            {
                string requestBody = reader.ReadToEnd();
                if (requestBody.Length > 0)
                    viewModel = JsonSerializer.Deserialize<Transaction>(requestBody);
            }

            if (viewModel == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error parsing the request parameters.");

            var dbModel = this.transactionsService.GetAll(x => x.Id == viewModel.Id).First();
            if (viewModel.Category != dbModel.Category && viewModel.Category?.Contains(":") == true)
            {
                var parts = viewModel.Category.Split(":");
                viewModel.Category = parts[0];
                var key = parts[1];
                this.categories.Add(new Category[] { new Category() { Name = parts[0], KeyWord = parts[1] } });
                var all = this.transactionsService.GetAll().ToList();
                new TransactionsClassifier().Classify(all, this.categories.GetAll());
                this.transactionsService.Update(all);
            }

            dbModel.Details = viewModel.Details;
            dbModel.Amount = viewModel.Amount;
            dbModel.Date = viewModel.Date;
            dbModel.Category = viewModel.Category ?? "";
            this.transactionsService.Update(new Transaction[] { dbModel });
            return new OkResult();
        }

        public IActionResult OnPostDelete(int id)
        {
            this.transactionsService.Remove(this.transactionsService.GetAll(x => x.Id == id));
            return new OkResult();
        }

        public void OnPostClassifyCurrent()
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
            this.OnGet();
        }

        public void OnPostDeleteFiltered()
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
            this.OnGet();
        }
    }

    public enum SortOptions
    {
        Date,
        Category,
        Amount
    }
}
