using ExpenseTracker.Core.Categories;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions.Rules;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages
{
    public class MigrateModel : PageModel
    {
        private readonly IBaseDataItemService<Category> categories;
        private readonly IBaseDataItemService<Rule> rules;

        public MigrateModel(IBaseDataItemService<Rule> rules, IBaseDataItemService<Category> categories)
        {
            this.rules = rules;
            this.categories = categories;
        }

        public void OnPostMigrateCategories()
        {
            var rulesToAdd = new List<Rule>();
            foreach (var cat in categories.GetAll().ToList())
            {
                var rule = new Rule()
                {
                    Property = "Details",
                    Condition = RuleCondition.Contains,
                    ConditionValue = cat.KeyWord,
                    PropertyToSet = "Category",
                    ValueToSet = cat.Name,
                    Action = RuleAction.SetProperty
                };

                rulesToAdd.Add(rule);
                this.categories.RemoveById(cat.Id);
            }

            this.rules.Add(rulesToAdd);
        }

        public void OnPostDeleteAllRules()
        {
            foreach (var r in rules.GetAll().ToList())
            {
                this.rules.RemoveById(r.Id);
            }
        }
    }
}