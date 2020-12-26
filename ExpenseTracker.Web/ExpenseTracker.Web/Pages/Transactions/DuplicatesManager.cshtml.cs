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

        public void OnPostMarkResolved(int index)
        {
            var targetIds = this.DuplicatesModel[index].Transactions.Select(x => x.Id);
            var dbModels = this.service.GetAll(x => targetIds.Contains(x.Id)).ToList();
            foreach (var item in dbModels)
            {
                item.IsResolvedDuplicate = true;
            }

            this.service.Update(dbModels);
            this.DuplicatesModel.RemoveAt(index);
            this.ModelState.Clear();
        }
    }
}
