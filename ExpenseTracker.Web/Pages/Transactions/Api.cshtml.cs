using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Transactions.Rules;
using ExpenseTracker.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class ApiModel : PageModel
    {
        private readonly ITransactionsService transactionsService;
        private readonly IGenericRepository<Rule> rules;

        public ApiModel(ITransactionsService transactions, IGenericRepository<Rule> rules)
        {
            this.transactionsService = transactions;
            this.rules = rules;
        }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        public IActionResult OnPost()
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
                this.rules.Insert(new Rule() { ValueToSet = parts[0], ConditionValue = parts[1], Condition = RuleCondition.Contains, Action = RuleAction.SetProperty, Property = "Details", PropertyToSet = "Category" });
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