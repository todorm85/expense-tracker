using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
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
        private readonly MailImporter importer;

        public UploadModel(ITransactionsService transactionsService, AllianzTxtFileParser allianz, RaiffeizenTxtFileParser rai, MailImporter importer)
        {
            this.transactionsService = transactionsService;
            this.allianz = allianz;
            this.rai = rai;
            this.importer = importer;
            this.Transactions = new List<Transaction>();
        }

        [BindProperty]
        public IList<IFormFile> Files { get; set; }

        [BindProperty]
        public Transaction CreateTransaction { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool Success { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowMessage { get; set; }

        [BindProperty]
        public List<Transaction> Transactions { get; set; }

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

            AddJustAdded(new Transaction[] { dbModel });
            this.CreateTransaction = new Transaction() { Date = DateTime.Now };
            Success = true;
            ShowMessage = true;
            return Page();
        }

        public IActionResult OnPostUpload(List<IFormFile> files)
        {
            try
            {
                IEnumerable<Transaction> expenses = Enumerable.Empty<Transaction>();
                IEnumerable<Transaction> added = Enumerable.Empty<Transaction>();
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
                            expenses = this.rai.ParseFile(filePath);
                            this.transactionsService.Add(expenses, out added);
                        }
                        else if (formFile.FileName.EndsWith("txt"))
                        {
                            expenses = this.allianz.GetTransactions(filePath);
                            this.transactionsService.Add(expenses, out added);
                        }
                    }
                }

                AddJustAdded(added);
                Success = true;
                ShowMessage = true;
                return Page();
            }
            catch (Exception)
            {
                return Page();
            }
        }

        public void OnPostSyncMail()
        {
            this.importer.ImportTransactions(out IEnumerable<Transaction> ts);
            AddJustAdded(ts);
        }

        private void AddJustAdded(IEnumerable<Transaction> ts)
        {
            this.Transactions = ts.Concat(this.Transactions).ToList();
            this.ModelState.Clear();
        }
    }
}
