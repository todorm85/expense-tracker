using ExpenseTracker.Core.Services;
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
        private readonly IExpensesService service;

        public DuplicatesManagerModel(IExpensesService transactions)
        {
            this.service = transactions;
        }

        [BindProperty]
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

            this.service.UpdateTransactions(targets);
            return new OkResult();
        }

        public IActionResult OnPostDeleteTransaction(string id)
        {
            service.RemoveTransaction(id);
            return RedirectToPage();
        }

        public IActionResult OnPostUpdateTransaction(string id)
        {
            foreach (var set in DuplicatesModel)
            {
                foreach (var t in set.Transactions)
                {
                    if (t.TransactionId == id)
                    {
                        service.UpdateTransaction(t);
                        return RedirectToPage();
                    }
                }
            }

            throw new ArgumentOutOfRangeException(nameof(id));
        }
    }
}