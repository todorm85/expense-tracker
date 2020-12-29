using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class DuplicatesManagerModel : PageModel
    {
        private readonly ITransactionsService service;

        public DuplicatesManagerModel(ITransactionsService transactions)
        {
            this.service = transactions;
        }

        public IList<TransactionsListModel> DuplicatesModel { get; set; }

        public void OnGet()
        {
            var duplicates = this.service.GetPotentialDuplicates();
            this.DuplicatesModel = duplicates.Select(x => new TransactionsListModel()
            {
                ShowId = true,
                DetailsHeight = 6,
                ShowTime = true,
                Transactions = x
            }).ToList();
        }

        public IActionResult OnPostMarkResolved([FromBody]string ids)
        {
            var targetIds = ids.Split(',').Select(x => int.Parse(x));
            var targets = this.service.GetAll(x => targetIds.Contains(x.Id)).ToList();
            foreach (var item in targets)
            {
                item.IsResolvedDuplicate = true;
            }

            this.service.Update(targets);
            return new OkResult();
        }
    }
}
