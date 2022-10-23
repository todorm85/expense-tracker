using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.Linq;

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
                DetailsHeight = 6,
                ShowTime = true,
                ShowSource = true,
                Transactions = x.Select(t => new TransactionModel(t)).ToList()
            }).ToList();
        }

        public IActionResult OnPostMarkResolved([FromBody] string ids)
        {
            var targetIds = ids.Split(',');
            var targets = this.service.GetAll(x => targetIds.Contains(x.TransactionId)).ToList();
            foreach (var item in targets)
            {
                item.IsResolvedDuplicate = true;
            }

            this.service.Update(targets);
            return new OkResult();
        }
    }
}