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
        private readonly ItemsFilterService<Rule, ItemsFilterResult<Rule>, ItemsFilterParams<Rule>> itemsFilter;


        public IndexModel(IExpensesService transactionsService, ItemsFilterService<Rule, ItemsFilterResult<Rule>, ItemsFilterParams<Rule>> itemsFilter)
        {
            this.transactionsService = transactionsService;
            this.itemsFilter = itemsFilter;

        }

        [BindProperty]
        public Rule CreateRuleModel { get; set; }

        [BindProperty(SupportsGet = true)]
        public int CurrentPage { get; set; }

        [BindProperty(SupportsGet = true)]
        public ItemsFilterParams<Rule> FilterParams { get; set; }

        [BindProperty]
        public List<Rule> Rules { get; set; }

        [BindProperty]
        public string Search { get; set; }
        public void OnGet()
        {
            InitRulesModel();
            InitCreateModel();
            this.FilterParams = this.FilterParams;
        }

        public IActionResult OnPost()
        {
            return RedirectToPage(new { CurrentPage, FilterParams });
        }

        public IActionResult OnPostDelete(int id)
        {
            this.transactionsService.RemoveRule(id);
            return RedirectToPage(new { CurrentPage, FilterParams });
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

            return RedirectToPage(new { CurrentPage, FilterParams });
        }

        public IActionResult OnPostProcessUncategorized()
        {
            transactionsService.ProcessAllUncategorizedTransactions();
            return RedirectToPage();
        }

        public IActionResult OnPostFilter()
        {
            return RedirectToPage(new { CurrentPage, FilterParams });
        }

        private void InitCreateModel()
        {
            this.CreateRuleModel = new Rule() { Property = "Details" };
        }

        private void InitRulesModel()
        {
            var filterExpression = GetFilterExpression();
            this.FilterParams = new ItemsFilterParams<Rule>()
            {
                PageIndex = CurrentPage,
                PageSize = itemsPerPage
            };
        }

        private Expression<Func<Rule, bool>> GetFilterExpression()
        {
            Expression<Func<Rule, bool>> filterExpression = null!;
            if (!string.IsNullOrWhiteSpace(Search))
            {
                if (Search != "skip")
                {
                    filterExpression = (x) => x.ConditionValue.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.ValueToSet.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.PropertyToSet.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.Property.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0;
                }
                else
                {
                    filterExpression = (x) => x.ConditionValue.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.ValueToSet.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.PropertyToSet.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.Property.IndexOf(Search, StringComparison.InvariantCultureIgnoreCase) >= 0
                            || x.Action == RuleAction.Skip;
                }
            }

            return filterExpression;
        }
    }
}