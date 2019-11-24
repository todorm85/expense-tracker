using System.Collections.Generic;
using ExpenseTracker.Core;

namespace ExpenseTracker.GmailConnector
{
    public class GmailClient
    {
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

            var messageParser = new ExpenseMessageParser();
            transactions = messageParser.Parse(expenseMessages);

            return transactions;
        }
    }
}
