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
        private readonly IBaseDataItemService<Rule> rulesService;

        public IndexModel(IBaseDataItemService<Rule> rules)
        {
            this.rulesService = rules;
            this.Rules = new List<Rule>();
            InitializeCreateRuleModel();
        }

        [BindProperty]
        public Rule CreateRuleModel { get; set; }

        [BindProperty]
        public IList<Rule> Rules { get; set; }

        public void OnGet()
        {
            this.Rules = this.rulesService.GetAll().ToList();
        }

        public void OnPostDelete(int id)
        {
            this.rulesService.RemoveById(id);
            this.Rules.Remove(this.Rules.First(x => x.Id == id));
            this.ModelState.Clear();
        }

        public void OnPostSave(int id)
        {
            if (id == default)
            {
                this.rulesService.Add(CreateRuleModel);
                this.Rules.Add(CreateRuleModel);
                InitializeCreateRuleModel();
            }
            else
            {
                var model = this.Rules.First(x => x.Id == id);
                this.rulesService.Update(model);
            }

            this.ModelState.Clear();
        }

        private void InitializeCreateRuleModel()
        {
            this.CreateRuleModel = new Rule() { Property = "Details" };
        }
    }
}