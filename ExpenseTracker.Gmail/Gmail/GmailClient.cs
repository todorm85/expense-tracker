using System.Collections.Generic;
using ExpenseTracker.Core;

namespace ExpenseTracker.Allianz
{
    public class GmailClient
    {
        private readonly IExpenseMessageParser parser;

        public GmailClient(IExpenseMessageParser parser)
        {
            this.parser = parser;
        }

        public IEnumerable<Transaction> Get(string user, string pass)
        {
            IEnumerable<Transaction> transactions;
            var expenseMessages = new List<ExpenseMessage>();
            using (var folder = new GmailFolder(user, pass))
            {
                while (folder.Count > 0)
                {
                    int msgIdx = 0;
                    var serverMsg = folder.GetMessage(msgIdx);
                    expenseMessages.Add(new ExpenseMessage()
                    {
                        Body = serverMsg.HtmlBody,
                        Subject = serverMsg.Subject,
                        EmailDate = serverMsg.Date.UtcDateTime
                    });

                    folder.Delete(msgIdx);
                }
            }

            transactions = this.parser.Parse(expenseMessages);

            return transactions;
        }
    }
}