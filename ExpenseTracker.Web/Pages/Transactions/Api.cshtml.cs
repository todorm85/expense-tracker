using ExpenseTracker.Core.Categories;
using ExpenseTracker.Core.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class ApiModel : PageModel
    {
        private readonly CategoriesService categories;
        private readonly ITransactionsService transactionsService;

        public ApiModel(ITransactionsService transactions, CategoriesService categories)
        {
            this.transactionsService = transactions;
            this.categories = categories;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        public IActionResult OnPostDelete(string id)
        {
            this.transactionsService.RemoveById(id);
            return new OkResult();
        }

        public IActionResult OnPostUpdate([FromBody] Transaction viewModel)
        {
            if (viewModel == null)
                return StatusCode(StatusCodes.Status500InternalServerError, "Error parsing the request parameters.");

            var dbModel = this.transactionsService.GetAll(x => x.TransactionId == viewModel.TransactionId).First();
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
    }
}