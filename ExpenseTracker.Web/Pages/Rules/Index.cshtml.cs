using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions.Rules;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Rules
{
    public class IndexModel : PageModel
    {
        private readonly IBaseDataItemService<Rule> rules;

        public IndexModel(IBaseDataItemService<Rule> rules)
        {
            this.rules = rules;
            this.Rules = Enumerable.Empty<Rule>();
        }

        public IEnumerable<Rule> Rules { get; set; }

        public void OnGet()
        {
            this.Rules = this.rules.GetAll();
        }
    }
}