using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Rules;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace ExpenseTracker.Web.Pages.Rules
{
    public class IndexModel : PageModel
    {
        private readonly int itemsPerPage = 10;
        private readonly IExpensesService transactionsService;

        public IndexModel(IExpensesService transactionsService)
        {
            this.transactionsService = transactionsService;
        }

        [BindProperty]
        public Rule CreateRuleModel { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; }

        [BindProperty]
        public PaginatedList<Rule> Rules { get; set; }

        [BindProperty(SupportsGet = true)]
        public string Filter { get; set; }

        public void OnGet()
        {
            InitRulesModel();
            InitCreateModel();
            this.Filter = this.Filter ?? string.Empty;
        }

        public IActionResult OnPost()
        {
            return RedirectToPage(new { CurrentPage, Filter });
        }

        public IActionResult OnPostDelete(int id)
        {
            this.transactionsService.RemoveRule(id);
            return RedirectToPage(new { CurrentPage, Filter });
        }

        public IActionResult OnPostSave(int id)
        {
            if (id == default)
            {
                this.transactionsService.CreateRule(CreateRuleModel);
            }
            else
            {
                var model = this.Rules.First(x => x.Id == id);
                this.transactionsService.UpdateRule(model);
            }

            return RedirectToPage(new { CurrentPage, Filter });
        }

        public IActionResult OnPostProcessUncategorized()
        {
            transactionsService.ProcessAllUncategorizedTransactions();
            return RedirectToPage();
        }

        public IActionResult OnPostFilter()
        {
            return RedirectToPage(new { CurrentPage, Filter });
        }

        private void InitCreateModel()
        {
            this.CreateRuleModel = new Rule() { Property = "Details" };
        }

        private void InitRulesModel()
        {
            var filterExpression = GetFilterExpression();
            this.Rules = new PaginatedList<Rule>(this.transactionsService, filterExpression, CurrentPage, itemsPerPage);
        }

        private Expression<Func<Rule, bool>> GetFilterExpression()
        {
            Expression<Func<Rule, bool>> filterExpression = null;
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                if (Filter != "skip")
                {
                    filterExpression = (x) => x.ConditionValue.IndexOf(Filter, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.ValueToSet.IndexOf(Filter, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.PropertyToSet.IndexOf(Filter, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.Property.IndexOf(Filter, StringComparison.InvariantCultureIgnoreCase) >= 0;
                }
                else
                {
                    filterExpression = (x) => x.ConditionValue.IndexOf(Filter, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.ValueToSet.IndexOf(Filter, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.PropertyToSet.IndexOf(Filter, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.Property.IndexOf(Filter, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.Action == RuleAction.Skip;
                }
            }

            return filterExpression;
        }
    }
}