using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions.Rules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Rules
{
    public class ApiModel : PageModel
    {
        private readonly IBaseDataItemService<Rule> rules;

        public ApiModel(IBaseDataItemService<Rule> rules)
        {
            this.rules = rules;
            this.Rules = Enumerable.Empty<Rule>();
        }

        public IEnumerable<Rule> Rules { get; set; }

        public IActionResult OnGet()
        {
            return NotFound();
        }

        public IActionResult OnPost()
        {
            return NotFound();
        }

        public IActionResult OnPostDelete(int id)
        {
            this.rules.RemoveById(id);
            return new OkResult();
        }

        public IActionResult OnPostSave([FromBody] Rule r)
        {
            if (r.Id == default)
            {
                this.rules.Add(r);
            }
            else
            {
                this.rules.Update(r);
            }

            return new OkResult();
        }
    }
}