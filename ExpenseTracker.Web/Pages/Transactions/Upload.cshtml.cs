using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.App;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Integrations.Files;
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
        private readonly RevolutExcelParser revolut;
        private readonly ITransactionsService transactionsService;

        public UploadModel(
            ITransactionsService transactionsService,
            AllianzTxtFileParser allianz,
            RaiffeizenTxtFileParser rai,
            RevolutExcelParser revolut,
            MailImporter importer,
            Config config)
        {
            this.transactionsService = transactionsService;
            this.allianz = allianz;
            this.rai = rai;
            this.importer = importer;
            this.revolut = revolut;
            this.TransactionsList = new TransactionsListModel() { ShowSource = true };
            this.SkippedTransactionsList = new TransactionsListModel() { ShowSource = true, ShowTime = true };
            this.HasMail = importer.TestConnection();
        }

        [BindProperty]
        public Transaction CreateTransaction { get; set; }

        [BindProperty]
        public IList<IFormFile> Files { get; set; }

        [BindProperty]
        public TransactionsListModel SkippedTransactionsList { get; set; }
        public bool HasMail { get; private set; }
        [BindProperty]
        public TransactionsListModel TransactionsList { get; set; }

        public void OnGet()
        {
            this.CreateTransaction = new Transaction() { Date = DateTime.Now, Type = TransactionType.Expense };
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
                Type = CreateTransaction.Type,
                Source = "manual_entry"
            };

            this.transactionsService.TryAdd(dbModel, out IEnumerable<TransactionInsertResult> skipped);

            AddJustAdded(new Transaction[] { dbModel });
            AddSkipped(skipped);
            this.CreateTransaction = new Transaction() { Date = DateTime.Now, Type = TransactionType.Expense };
            return Page();
        }

        public void OnPostSyncMail()
        {
            this.importer.ImportTransactions(out IEnumerable<Transaction> added, out IEnumerable<TransactionInsertResult> skipped);
            AddJustAdded(added);
            AddSkipped(skipped);
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
                        else if (formFile.FileName.EndsWith("csv"))
                        {
                            expenses = this.revolut.ParseFromFile(filePath);
                            this.transactionsService.TryAdd(expenses, out skipped);
                        }
                    }
                }

                AddJustAdded(expenses.Except(skipped.Select(x => x.Transaction)));
                AddSkipped(skipped);
                return Page();
            }
            catch (Exception e)
            {
                this.ViewData["errorMessage"] = e.Message + (e.InnerException != null ? e.InnerException.Message : "");
                return Page();
            }
        }

        private void AddJustAdded(IEnumerable<Transaction> ts)
        {
            var all = ts.Select(t => new TransactionModel(t)).Concat(TransactionsList.Transactions).ToList();
            this.TransactionsList.Transactions = all;
            this.ModelState.Clear();
        }

        private void AddSkipped(IEnumerable<TransactionInsertResult> skipped)
        {
            var all = skipped.Select(t => new TransactionModel(t.Transaction) { Reason = t.ReasonResult, TransactionId = Guid.NewGuid().ToString() }).Concat(this.SkippedTransactionsList.Transactions).ToList();
            this.SkippedTransactionsList.Transactions = all;
            this.ModelState.Clear();
        }
    }
}