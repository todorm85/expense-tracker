using ExpenseTracker.Core;
using System;
using System.Collections.Generic;

namespace ExpenseTracker.Allianz.Gmail
{
    public class MailImporter : IDisposable
    {
        private readonly IMailClient mailClient;
        private readonly IEnumerable<IExpenseMessageParser> messageParsers;
        private readonly ITransactionsService transactionsService;

        public MailImporter(IExpenseMessageParser[] parsers, ITransactionsService service, IMailClient mailClientFact)
        {
            this.messageParsers = parsers;
            this.transactionsService = service;
            this.mailClient = mailClientFact;
        }

        public void Dispose()
        {
            this.mailClient.Dispose();
        }

        public void ImportTransactions(out IEnumerable<Transaction> added)
        {
            added = new List<Transaction>();
            var transactions = new List<Transaction>();
            int msgIdx = 0;
            int totalMsgsCount = this.mailClient.Count;
            while (msgIdx < totalMsgsCount)
            {
                var expenseMessage = this.mailClient.GetMessage(msgIdx);
                Transaction t = null;
                foreach (var p in this.messageParsers)
                {
                    t = p.Parse(expenseMessage);
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

            if (transactions.Count > 0)
            {
                this.transactionsService.Add(transactions, out added);
            }
        }
    }
}