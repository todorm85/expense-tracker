using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions.Rules;
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
        private readonly IGenericRepository<Rule> rulesService;

        public IndexModel(IGenericRepository<Rule> rules)
        {
            this.rulesService = rules;
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
            this.rulesService.RemoveById(id);
            return RedirectToPage(new { CurrentPage, Filter });
        }

        public IActionResult OnPostSave(int id)
        {
            if (id == default)
            {
                this.rulesService.Insert(CreateRuleModel);
            }
            else
            {
                var model = this.Rules.First(x => x.Id == id);
                this.rulesService.Update(model);
            }

            return RedirectToPage(new { CurrentPage, Filter });
        }

        private void InitCreateModel()
        {
            this.CreateRuleModel = new Rule() { Property = "Details" };
        }

        private void InitRulesModel()
        {
            var filterExpression = GetFilterExpression();
            this.Rules = new PaginatedList<Rule>(this.rulesService, filterExpression, CurrentPage, itemsPerPage);
        }

        private Expression<Func<Rule, bool>> GetFilterExpression()
        {
            Expression<Func<Rule, bool>> filterExpression = null;
            if (!string.IsNullOrWhiteSpace(Filter))
            {
                if (Filter != "skip")
                {
                    filterExpression = (x) => x.ConditionValue.Contains(Filter)
                            || x.ValueToSet.Contains(Filter)
                            || x.PropertyToSet.Contains(Filter)
                            || x.Property.Contains(Filter);
                }
                else
                {
                    filterExpression = (x) => x.ConditionValue.Contains(Filter)
                            || x.ValueToSet.Contains(Filter)
                            || x.PropertyToSet.Contains(Filter)
                            || x.Property.Contains(Filter)
                            || x.Action == RuleAction.Skip;
                }
            }

            return filterExpression;
        }
    }
}