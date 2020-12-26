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

        [BindProperty]
        public IList<TransactionsListModel> DuplicatesModel { get; set; }

        public void OnGet()
        {
            var duplicates = this.service.GetDuplicates();
            this.DuplicatesModel = duplicates.Select(x => new TransactionsListModel()
            {
                ShowId = true,
                DetailsHeight = 6,
                ShowTime = true,
                Transactions = x
            }).ToList();
        }

        public IActionResult OnPostMarkResolved(int index)
        {
            var targetIds = this.DuplicatesModel[index].Transactions.ToList();
            foreach (var item in targetIds)
            {
                item.IsResolvedDuplicate = true;
            }

            this.service.Update(targetIds);
            return RedirectToPage();
        }
    }
}
