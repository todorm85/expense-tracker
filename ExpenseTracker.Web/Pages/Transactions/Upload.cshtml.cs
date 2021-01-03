using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.Core.Transactions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class UploadModel : PageModel
    {
        private readonly AllianzTxtFileParser allianz;
        private readonly MailImporter importer;
        private readonly RaiffeizenTxtFileParser rai;
        private readonly ITransactionsService transactionsService;

        public UploadModel(ITransactionsService transactionsService, AllianzTxtFileParser allianz, RaiffeizenTxtFileParser rai, MailImporter importer)
        {
            this.transactionsService = transactionsService;
            this.allianz = allianz;
            this.rai = rai;
            this.importer = importer;
            this.TransactionsList = new TransactionsListModel();
        }

        [BindProperty]
        public Transaction CreateTransaction { get; set; }

        [BindProperty]
        public IList<IFormFile> Files { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool ShowMessage { get; set; }

        [BindProperty(SupportsGet = true)]
        public bool Success { get; set; }

        [BindProperty]
        public TransactionsListModel TransactionsList { get; set; }

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

            this.transactionsService.TryAdd(dbModel, out IEnumerable<TransactionInsertResult> skipped);

            AddJustAdded(new Transaction[] { dbModel });
            this.CreateTransaction = new Transaction() { Date = DateTime.Now };
            Success = true;
            ShowMessage = true;
            return Page();
        }

        public void OnPostSyncMail()
        {
            this.importer.ImportTransactions(out IEnumerable<Transaction> added, out IEnumerable<TransactionInsertResult> skipped);
            AddJustAdded(added);
        }

        public IActionResult OnPostUpload(List<IFormFile> files)
        {
            try
            {
                IEnumerable<Transaction> expenses = Enumerable.Empty<Transaction>();
                IEnumerable<TransactionInsertResult> skipped = new List<TransactionInsertResult>();
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
                            this.transactionsService.TryAdd(expenses, out skipped);
                        }
                        else if (formFile.FileName.EndsWith("txt"))
                        {
                            expenses = this.allianz.ParseFromFile(filePath);
                            this.transactionsService.TryAdd(expenses, out skipped);
                        }
                    }
                }

                AddJustAdded(expenses.Except(skipped.Select(x => x.Transaction)));
                Success = true;
                ShowMessage = true;
                return Page();
            }
            catch (Exception)
            {
                return Page();
            }
        }

        private void AddJustAdded(IEnumerable<Transaction> ts)
        {
            this.TransactionsList.Transactions = ts.Concat(this.TransactionsList.Transactions).ToList();
            this.ModelState.Clear();
        }
    }
}