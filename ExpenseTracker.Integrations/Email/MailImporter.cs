using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Services.Models;
using ExpenseTracker.Core.Transactions;
using Microsoft.Extensions.Caching.Memory;
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

        public MailImporter(IExpenseMessageParser[] parsers, IExpensesService service, IMailClient mailClientFact, IMemoryCache cache)
        {
            this.messageParsers = parsers;
            this.transactionsService = service;
            this.mailClient = mailClientFact;
            this._cache = cache;
        }

        public void Dispose()
        {
            this.mailClient.Dispose();
        }

        public void ImportTransactions(out IEnumerable<Transaction> added, out IEnumerable<CreateTransactionResult> skipped)
        {
            added = Enumerable.Empty<Transaction>();
            skipped = Enumerable.Empty<CreateTransactionResult>();
            var transactions = TryParseMails();
            if (transactions.Count > 0)
            {
                this.transactionsService.TryCreateTransactions(transactions, out IEnumerable<CreateTransactionResult> skippedFromTry);
                skipped = skippedFromTry;
                added = transactions.Except(skippedFromTry.Select(x => x.Transaction));
            }
        }

        private List<Transaction> TryParseMails()
        {
            List<Transaction> transactions = new List<Transaction>();
            try
            {
                ParseMails(transactions);
            }
            catch (Exception)
            {
                // TODO: perhaps display a message that error has occured or log somewhere
            }

            return transactions;
        }

        private void ParseMails(List<Transaction> transactions)
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
                    catch (Exception)
                    {
                        t = null;
                    }

                    if (t != null)
                    {
                        break;
                    }
                }

                if (t != null)
                {
                    transactions.Add(t);
                    this.mailClient.Delete(msgIdx);
                    totalMsgsCount--;
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