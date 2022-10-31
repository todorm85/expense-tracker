using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.App;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Rules;
using ExpenseTracker.Integrations.Files;
using ExpenseTracker.Web.Pages.Shared;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.Web.Session;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Services.Models;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class UploadModel : PageModel
    {
        private readonly AllianzTxtFileParser allianz;
        private readonly MailImporter importer;
        private readonly RaiffeizenTxtFileParser rai;
        private readonly RevolutExcelParser revolut;
        private readonly IExpensesService transactionsService;

        public UploadModel(
            IExpensesService transactionsService,
            AllianzTxtFileParser allianz,
            RaiffeizenTxtFileParser rai,
            RevolutExcelParser revolut,
            MailImporter importer)
        {
            this.transactionsService = transactionsService;
            this.allianz = allianz;
            this.rai = rai;
            this.importer = importer;
            this.revolut = revolut;
            this.HasMail = importer.TestConnection();
        }

        [BindProperty]
        public Transaction NewTransaction { get; set; }

        [BindProperty]
        public IList<IFormFile> Files { get; set; }

        [BindProperty]
        public TransactionsListModel SkippedTransactions { get; set; } = new TransactionsListModel() { ShowSource = true };

        public bool HasMail { get; private set; }

        [BindProperty]
        public TransactionsListModel JustAddedTransactions { get; set; } = new TransactionsListModel() { ShowSource = true };

        public void OnGet()
        {
            this.NewTransaction = new Transaction() 
            { 
                Date = DateTime.Now,
                Type = TransactionType.Expense,
                TransactionId = Guid.NewGuid().ToString(),
                Source = "manual_entry"
            };
            var justAdded = GetJustAdded();
            if (justAdded != null)
                JustAddedTransactions.Transactions = justAdded;
            var skipped = GetSkipped();
            if (skipped != null)
                SkippedTransactions.Transactions = skipped;
        }

        public IActionResult OnPostCreate()
        {
            if (this.transactionsService.TryCreateTransaction(NewTransaction, out IEnumerable<CreateTransactionResult> skipped))
                SetJustAdded(JustAddedTransactions.Transactions.Concat(new List<Transaction>() { NewTransaction }).ToTransactionModel());
            else
                AppendSkipped(skipped.ToTransactionModel());

            return RedirectToPage();
        }

        public IActionResult OnPostSyncMail()
        {
            this.importer.ImportTransactions(out IEnumerable<Transaction> added, out IEnumerable<CreateTransactionResult> skipped);
            AppendJustAdded(added.ToTransactionModel());
            AppendSkipped(skipped.ToTransactionModel());
            return RedirectToPage();
        }

        public IActionResult OnPostUpload(List<IFormFile> files)
        {
            try
            {
                IEnumerable<Transaction> expenses = Enumerable.Empty<Transaction>();
                IEnumerable<CreateTransactionResult> skipped = new List<CreateTransactionResult>();
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
                            this.transactionsService.TryCreateTransactions(expenses, out skipped);
                        }
                        else if (formFile.FileName.EndsWith("txt"))
                        {
                            expenses = this.allianz.ParseFromFile(filePath);
                            this.transactionsService.TryCreateTransactions(expenses, out skipped);
                        }
                        else if (formFile.FileName.EndsWith("csv"))
                        {
                            expenses = this.revolut.ParseFromFile(filePath);
                            this.transactionsService.TryCreateTransactions(expenses, out skipped);
                        }
                    }
                }

                SetJustAdded(expenses.Except(skipped.Select(x => x.Transaction)).ToTransactionModel());
                SetSkipped(skipped.ToTransactionModel());
                return RedirectToPage();
            }
            catch (Exception e)
            {
                this.ViewData["errorMessage"] = e.Message + (e.InnerException != null ? e.InnerException.Message : "");
                return Page();
            }
        }

        public IActionResult OnPostDeleteTransaction(string id)
        {
            transactionsService.RemoveTransaction(id);
            SetJustAdded(JustAddedTransactions.Transactions.Where(x => x.TransactionId != id));
            return RedirectToPage();
        }

        public IActionResult OnPostUpdateTransaction(string id)
        {
            var updated = JustAddedTransactions.Transactions.First(x => x.TransactionId == id);
            transactionsService.UpdateTransaction(updated);
            SetJustAdded(JustAddedTransactions.Transactions);
            return RedirectToPage();
        }

        public IActionResult OnPostClearAdded()
        {
            SetJustAdded(null);
            return RedirectToPage();
        }

        public IActionResult OnPostClearSkipped()
        {
            SetSkipped(null);
            return RedirectToPage();
        }

        private void SetJustAdded(IEnumerable<TransactionModel> ts)
        {
            HttpContext.Session.Set("justAdded", ts);
        }

        private IList<TransactionModel> GetJustAdded()
        {
            return HttpContext.Session.Get<IList<TransactionModel>>("justAdded");
        }

        private void AppendSkipped(IEnumerable<TransactionModel> skipped)
        {
            SetSkipped(SkippedTransactions.Transactions.Concat(skipped));
        }

        private void AppendJustAdded(IEnumerable<TransactionModel> added)
        {
            SetJustAdded(JustAddedTransactions.Transactions.Concat(added));
        }

        private void SetSkipped(IEnumerable<TransactionModel> skipped)
        {
            HttpContext.Session.Set("skipTrasnaction", skipped);
        }

        private IList<TransactionModel> GetSkipped()
        {
            return HttpContext.Session.Get<IList<TransactionModel>>("skipTrasnaction");
        }
    }

    internal static class Extensions
    {
        public static IEnumerable<TransactionModel> ToTransactionModel(this IEnumerable<CreateTransactionResult> skipped)
        {
            return skipped.Select(x => new TransactionModel(x.Transaction) { Reason = x.ReasonResult, TransactionId = Guid.NewGuid().ToString() });
        }

        public static IEnumerable<TransactionModel> ToTransactionModel(this IEnumerable<Transaction> ts)
        {
            return ts.Select(x => new TransactionModel(x));
        }
    }
}