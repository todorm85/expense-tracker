using System;
using System.Collections.Generic;
using System.IO;
using ExpenseTracker.Allianz;
using ExpenseTracker.Core;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class UploadModel : PageModel
    {
        private readonly ITransactionsService transactionsService;
        private readonly AllianzTxtFileParser allianz;
        private readonly RaiffeizenTxtFileParser rai;

        public UploadModel(ITransactionsService transactionsService, AllianzTxtFileParser allianz, RaiffeizenTxtFileParser rai)
        {
            this.transactionsService = transactionsService;
            this.allianz = allianz;
            this.rai = rai;
        }

        [BindProperty]
        public IList<IFormFile> Files { get; set; }

        [BindProperty]
        public Transaction CreateTransaction { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool Success { get; set; }

        public void OnGet()
        {
            this.CreateTransaction = new Transaction() { Date = DateTime.Now };
        }

        public IActionResult OnPostCreate()
        {
            var dbModel = new Transaction()
            {
                TransactionId = Guid.NewGuid().ToString(),
                Amount = CreateTransaction.Amount,
                Category = CreateTransaction.Category,
                Date = CreateTransaction.Date,
                Details = CreateTransaction.Details,
                Type = CreateTransaction.Type
            };

            this.transactionsService.Add(dbModel);
            return RedirectToPage();
        }

        public IActionResult OnPostUpload(List<IFormFile> files)
        {
            try
            {
                foreach (var formFile in files)
                {
                    if (formFile.Length > 0)
                    {
                        var filePath = Path.GetTempFileName();

                        using (var stream = System.IO.File.Create(filePath))
                        {
                            formFile.CopyTo(stream);
                        }

                        if (formFile.FileName.EndsWith("xml"))
                        {
                            IEnumerable<Transaction> expenses = this.rai.ParseFile(filePath);
                            this.transactionsService.Add(expenses);
                        }
                        else if (formFile.FileName.EndsWith("txt"))
                        {
                            IEnumerable<Transaction> expenses = this.allianz.GetTransactions(filePath);
                            this.transactionsService.Add(expenses);
                        }
                    }
                }

                return RedirectToPage(new { Success = true });
            }
            catch (Exception)
            {
                return RedirectToPage();
            }
        }
    }
}
