using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Services.Models;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Integrations;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Allianz.Gmail
{
    public class MailImporter : IDisposable
    {
        private readonly IMemoryCache _cache;
        private readonly TimeSpan _cacheDuration = TimeSpan.FromMinutes(5);

        private readonly IMailClient mailClient;
        private readonly IEnumerable<IExpenseMessageParser> messageParsers;
        private readonly IExpensesService transactionsService;
        private readonly ILogger<MailImporter> _logger;

        public MailImporter(IEnumerable<IExpenseMessageParser> parsers, IExpensesService service, IMailClient mailClientFact, IMemoryCache cache, ILogger<MailImporter> logger)
        {
            this.messageParsers = parsers;
            this.transactionsService = service;
            this.mailClient = mailClientFact;
            this._cache = cache;
            this._logger = logger;
        }

        public bool DeleteMailAfterImport { get; set; } = false;

        public void Dispose()
        {
            this.mailClient.Dispose();
        }

        public void ImportTransactions(out IEnumerable<Transaction> added, out IEnumerable<CreateTransactionResult> skipped, out IEnumerable<ImportError> errors)
        {
            added = Enumerable.Empty<Transaction>();
            skipped = Enumerable.Empty<CreateTransactionResult>();
            errors = Enumerable.Empty<ImportError>();
            
            var result = TryParseMails();
            var transactions = result.Transactions;
            errors = result.Errors;
            
            if (transactions.Count > 0)
            {
                this.transactionsService.TryCreateTransactions(transactions, out IEnumerable<CreateTransactionResult> skippedFromTry);
                skipped = skippedFromTry;
                added = transactions.Except(skippedFromTry.Select(x => x.Transaction));
            }
        }

        private (List<Transaction> Transactions, List<ImportError> Errors) TryParseMails()
        {
            List<Transaction> transactions = new List<Transaction>();
            List<ImportError> errors = new List<ImportError>();
            try
            {
                ParseMails(transactions, errors);
            }
            catch (Exception ex)
            {
                var importError = new ImportError(
                    "Unexpected error during mail parsing", 
                    ImportErrorType.OtherError, 
                    ex, 
                    "Email Import");
                    
                errors.Add(importError);
                if (_logger != null)
                {
                    _logger.LogError(ex, "Unexpected error during mail parsing");
                }
            }

            return (transactions, errors);
        }

        private void ParseMails(List<Transaction> transactions, List<ImportError> errors)
        {
            int msgIdx = 0;
            int totalMsgsCount = this.mailClient.Count;
            while (msgIdx < totalMsgsCount)
            {
                var expenseMessage = this.mailClient.GetMessage(msgIdx);
                Transaction t = null;
                foreach (var p in this.messageParsers)
                {
                    try
                    {
                        t = p.Parse(expenseMessage);
                    }
                    catch (Exception e)
                    {
                        t = null;
                        var parserName = p.GetType().Name;
                        var importError = new ImportError(
                            $"Failed to parse email with parser {parserName}: {e.Message}", 
                            ImportErrorType.ParseError, 
                            e, 
                            $"Email: {expenseMessage.Subject ?? "No subject"}");
                            
                        errors.Add(importError);
                        if (_logger != null)
                        {
                            _logger.LogError(e, "Failed to parse expense message with {Parser}: {ExpenseMessage}", 
                                parserName, expenseMessage);
                        }
                    }
                        
                    if (t != null)
                    {
                        break;
                    }
                }

                if (t != null)
                {
                    transactions.Add(t);
                    if (DeleteMailAfterImport)
                    {
                        bool deletionSucceeded = false;
                        try
                        {
                            this.mailClient.Delete(msgIdx);
                            deletionSucceeded = true;
                        }
                        catch (Exception ex)
                        {
                            _logger?.LogError(ex, "Failed to delete email after import");
                            errors.Add(new ImportError(
                                "Failed to delete email after import", 
                                ImportErrorType.OtherError, 
                                ex, 
                                "Email Deletion"));
                        }

                        if (deletionSucceeded)
                        {
                            totalMsgsCount--;
                        }
                        else
                        {
                            msgIdx++;
                        }
                    }
                    else
                    {
                        msgIdx++;
                    }
                }
                else
                {
                    msgIdx++;
                }
            }
        }

        public bool TestConnection()
        {
            return _cache.GetOrCreate("MailConnectionStatus", entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = _cacheDuration;
                return this.mailClient.TestConnection();
            });
        }
    }
}