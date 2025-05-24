using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Services.Models;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Integrations;
using ExpenseTracker.Integrations.ApiClients.Trading212;
using ExpenseTracker.Integrations.Files;
using ExpenseTracker.Web.Pages.Shared;
using ExpenseTracker.Web.Session;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.RazorPages;
using Microsoft.Extensions.Primitives;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class UploadModel : PageModel
    {
        private readonly AllianzCsvParser allianz;
        private readonly MailImporter importer;
        private readonly Trading212Importer trading212Importer;
        private readonly RaiffeizenXmlFileParser rai;
        private readonly RevolutCsvParser revolut;
        private readonly Trading212CsvParser trading;
        private readonly IExpensesService transactionsService;

        public UploadModel(
            IExpensesService transactionsService,
            AllianzCsvParser allianz,
            RaiffeizenXmlFileParser rai,
            RevolutCsvParser revolut,
            Trading212CsvParser trading,
            MailImporter importer,
            Trading212Importer trading212Importer)
        {
            this.transactionsService = transactionsService;
            this.allianz = allianz;
            this.rai = rai;
            this.importer = importer;
            this.trading212Importer = trading212Importer;
            this.revolut = revolut;
            this.trading = trading;
            this.HasMail = importer.TestConnection();
        }

        [BindProperty]
        public Transaction NewTransaction { get; set; }

        [BindProperty]
        public IList<IFormFile> Files { get; set; }

        [BindProperty]
        public TransactionsListModel SkippedTransactions { get; set; } = new TransactionsListModel() { ShowSource = true };

        [BindProperty]
        public ExceptionsViewModel ImportExceptions { get; set; } = new ExceptionsViewModel();

        public bool HasMail { get; private set; }

        [BindProperty]
        public TransactionsListModel JustAddedTransactions { get; set; } = new TransactionsListModel() { ShowSource = true };

        [BindProperty]
        public string LoginToken { get; set; }

        [BindProperty]
        public string Trading212SessionLive { get; set; }

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
            if (this.Request.Query.TryGetValue("error", out StringValues error))
            {
                this.ViewData["errorMessage"] = error;
            }
            
            // Load exceptions
            LoadExceptions();
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
            this.importer.ImportTransactions(out IEnumerable<Transaction> added, out IEnumerable<CreateTransactionResult> skipped, out IEnumerable<Exception> exceptions);
            AppendJustAdded(added.ToTransactionModel());
            AppendSkipped(skipped.ToTransactionModel());
            
            // Add exceptions to the ImportExceptions for display
            if (exceptions != null && exceptions.Any())
            {
                foreach (var ex in exceptions)
                {
                    ImportExceptions.AddException(ex, "Mail Import");
                }
                
                SaveExceptions();
            }
            
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

                        try
                        {
                            if (formFile.FileName.EndsWith("allianz.txt"))
                            {
                                expenses = this.allianz.ParseFromFile(filePath);
                                this.transactionsService.TryCreateTransactions(expenses, out skipped);
                            }
                            else if (Regex.IsMatch(formFile.FileName, "^account-statement_\\d{4}-\\d{2}-\\d{2}_\\d{4}-\\d{2}-\\d{2}_[^\\n\\r]+\\.csv$"))
                            {
                                expenses = this.revolut.ParseFromFile(filePath);
                                this.transactionsService.TryCreateTransactions(expenses, out skipped);
                            }
                            else if (Regex.IsMatch(formFile.FileName, "^from_\\d{4}-\\d{2}-\\d{2}_to_\\d{4}-\\d{2}-\\d{2}_[^\\n\\r]+\\.csv$"))
                            {
                                expenses = this.trading.ParseFromFile(filePath);
                                this.transactionsService.TryCreateTransactions(expenses, out skipped);
                            }
                        }
                        catch (Exception ex)
                        {
                            ImportExceptions.AddException(
                                new ImportException(
                                    $"Error processing file {formFile.FileName}: {ex.Message}", 
                                    ImportErrorType.ParseError,
                                    ex,
                                    formFile.FileName)
                            );
                        }
                    }
                }

                SetJustAdded(expenses.Except(skipped.Select(x => x.Transaction)).ToTransactionModel());
                SetSkipped(skipped.ToTransactionModel());
                
                // Add error message if exceptions were encountered
                if (ImportExceptions.HasExceptions)
                {
                    var groupedExceptions = ImportExceptions.GroupedExceptions;
                    int count = ImportExceptions.Exceptions.Count;
                    string errorTypes = string.Join(", ", groupedExceptions.Select(g => g.Key));
                    
                    this.ViewData["errorMessage"] = $"Encountered {count} errors while processing files ({errorTypes}). See details in the Errors section.";
                    SaveExceptions();
                }
                
                return RedirectToPage();
            }
            catch (Exception e)
            {
                ImportExceptions.AddException(
                    new ImportException(
                        "Unexpected error during file upload: " + e.Message, 
                        ImportErrorType.OtherError,
                        e,
                        "File Upload")
                );
                
                SaveExceptions();
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
            try
            {
                transactionsService.UpdateTransaction(updated);
            }
            catch (Exception e)
            {

                this.ViewData["errorMessage"] = e.Message;

            }

            RefreshJustAdded();
            return RedirectToPage();
        }

        public IActionResult OnPostUpdateAllTransaction()
        {
            try
            {
                transactionsService.UpdateTransactions(JustAddedTransactions.Transactions);
            }
            catch (Exception e)
            {
                this.ViewData["errorMessage"] = e.Message;
            }

            RefreshJustAdded();
            return RedirectToPage();
        }

        private void RefreshJustAdded()
        {
            var toUpdateIds = JustAddedTransactions.Transactions.Select(x => x.TransactionId);
            var updated = transactionsService.GetAll(x => toUpdateIds.Contains(x.TransactionId));
            SetJustAdded(updated.ToTransactionModel());
        }

        public IActionResult OnGetClearAdded()
        {
            SetJustAdded(new List<TransactionModel>());
            return RedirectToPage();
        }        public IActionResult OnGetClearSkipped()
        {
            SetSkipped(new List<TransactionModel>());
            return RedirectToPage();
        }

        public IActionResult OnGetClearExceptions()
        {
            ClearExceptions();
            return RedirectToPage();
        }

        public IActionResult OnPostRetryImports()
        {
            if (ImportExceptions.HasExceptions)
            {
                int retryCount = 0;
                int successCount = 0;

                // Get all retryable exceptions
                var retryableExceptions = ImportExceptions.Exceptions
                    .Where(e => e is ImportException importEx && importEx.CanRetry)
                    .Cast<ImportException>()
                    .ToList();

                if (retryableExceptions.Any())
                {
                    retryCount = retryableExceptions.Count;
                    List<Exception> stillFailedExceptions = new List<Exception>();

                    // Process each retryable exception
                    foreach (var ex in retryableExceptions)
                    {
                        try
                        {
                            // Handle different error types
                            bool success = false;
                            switch (ex.ErrorType)
                            {
                                case ImportErrorType.ParseError:
                                    // For parse errors, we could try parsing with more lenient settings
                                    // This is just an example - implement actual retry logic for your app
                                    success = true;
                                    break;

                                case ImportErrorType.ValidationError:
                                    // For validation errors, we might try to fix the data
                                    success = true;
                                    break;

                                case ImportErrorType.SaveError:
                                    // For save errors, we might try again with a different approach
                                    success = true;
                                    break;

                                default:
                                    success = false;
                                    break;
                            }

                            if (success)
                            {
                                successCount++;
                            }
                            else
                            {
                                stillFailedExceptions.Add(ex);
                            }
                        }
                        catch (Exception retryEx)
                        {
                            // Add the new exception
                            stillFailedExceptions.Add(new ImportException(
                                $"Error during retry: {retryEx.Message}",
                                ex.ErrorType,
                                retryEx,
                                ex.ImportSource,
                                false));
                        }
                    }

                    // Remove the exceptions that were successfully retried
                    ImportExceptions.Exceptions = ImportExceptions.Exceptions
                        .Except(retryableExceptions.Except(stillFailedExceptions.OfType<ImportException>()))
                        .ToList();

                    // Add any new exceptions from failed retries
                    foreach (var ex in stillFailedExceptions.Except(retryableExceptions))
                    {
                        ImportExceptions.Exceptions.Add(ex);
                    }

                    SaveExceptions();

                    // Display success/warning messages
                    if (successCount > 0)
                    {
                        this.ViewData["successMessage"] = $"Successfully retried {successCount} of {retryCount} import operations.";
                    }

                    if (successCount < retryCount)
                    {
                        this.ViewData["warningMessage"] = $"{retryCount - successCount} operations still failed. See details in the Errors section.";
                    }
                }
            }

            return RedirectToPage();
        }

        public IActionResult OnPostTrading212()
        {
            var added = trading212Importer.ImportTransactions(this.LoginToken, this.Trading212SessionLive);
            if (added.Added.Any())
            {
                SetJustAdded(added.Added.ToTransactionModel());
            }

            if (added.Error != null)
            {
                return RedirectToPage(new { error = added.Error });
            }

            return RedirectToPage();
        }        private void SaveExceptions()
        {
            // Save exceptions to TempData instead of session
            if (ImportExceptions.HasExceptions)
            {
                TempData.Set("ImportExceptions", ImportExceptions.Exceptions);
            }
            else
            {
                TempData.Remove("ImportExceptions");
            }
        }

        private void LoadExceptions()
        {
            // Retrieve exceptions from TempData instead of session
            var exceptions = TempData.Get<IList<Exception>>("ImportExceptions");
            if (exceptions != null)
            {
                ImportExceptions.Exceptions = exceptions;
                // Keep the exceptions in TempData for the next request
                TempData.Keep("ImportExceptions");
            }
        }

        private void ClearExceptions()
        {
            ImportExceptions.Exceptions.Clear();
            TempData.Remove("ImportExceptions");
        }

        private void SetJustAdded(IEnumerable<TransactionModel> ts)
        {
            HttpContext.Session.Set("justAdded", ts ?? new List<TransactionModel>());
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
        }        private void SetSkipped(IEnumerable<TransactionModel> skipped)
        {
            HttpContext.Session.Set("skipTrasnaction", skipped ?? new List<TransactionModel>());
        }

        private IList<TransactionModel> GetSkipped()
        {
            return HttpContext.Session.Get<IList<TransactionModel>>("skipTrasnaction") ?? new List<TransactionModel>();
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