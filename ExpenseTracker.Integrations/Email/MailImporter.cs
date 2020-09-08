using System;
using System.Collections.Generic;
using ExpenseTracker.Core;

namespace ExpenseTracker.Allianz.Gmail
{
    public class MailImporter : IDisposable
    {
        private readonly IEnumerable<IExpenseMessageParser> messageParsers;
        private readonly ITransactionsService transactionsService;
        private readonly IMailClient mailClient;

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

        public void ImportTransactions()
        {
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
                this.transactionsService.Add(transactions);
            }
        }
    }
}