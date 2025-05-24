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

        public bool DeleteMailAfterImport { get; set; } = true;

        public void Dispose()
        {
            this.mailClient.Dispose();
        }

        public void ImportTransactions(out IEnumerable<Transaction> added, out IEnumerable<CreateTransactionResult> skipped, out IEnumerable<Exception> exceptions)
        {
            added = Enumerable.Empty<Transaction>();
            skipped = Enumerable.Empty<CreateTransactionResult>();
            exceptions = Enumerable.Empty<Exception>();
            
            var result = TryParseMails();
            var transactions = result.Transactions;
            exceptions = result.Exceptions;
            
            if (transactions.Count > 0)
            {
                this.transactionsService.TryCreateTransactions(transactions, out IEnumerable<CreateTransactionResult> skippedFromTry);
                skipped = skippedFromTry;
                added = transactions.Except(skippedFromTry.Select(x => x.Transaction));
            }
        }

        private (List<Transaction> Transactions, List<Exception> Exceptions) TryParseMails()
        {
            List<Transaction> transactions = new List<Transaction>();
            List<Exception> exceptions = new List<Exception>();
            try
            {
                ParseMails(transactions, exceptions);
            }
            catch (Exception ex)
            {
                var importEx = new ImportException(
                    "Unexpected error during mail parsing", 
                    ImportErrorType.OtherError, 
                    ex, 
                    "Email Import");
                    
                exceptions.Add(importEx);
                if (_logger != null)
                {
                    _logger.LogError(ex, "Unexpected error during mail parsing");
                }
            }

            return (transactions, exceptions);
        }

        private void ParseMails(List<Transaction> transactions, List<Exception> exceptions)
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
                        var importEx = new ImportException(
                            $"Failed to parse email with parser {parserName}: {e.Message}", 
                            ImportErrorType.ParseError, 
                            e, 
                            $"Email: {expenseMessage.Subject ?? "No subject"}");
                            
                        exceptions.Add(importEx);
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
                            exceptions.Add(new ImportException(
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