using System.Collections.Generic;
using ExpenseTracker.Core;

namespace ExpenseTracker.GmailConnector
{
    public class GmailClient
    {
        public string User { get; set; }
        public string Pass { get; set; }

        public IEnumerable<Transaction> Get()
        {
            IEnumerable<Transaction> expenses;
            using (var folder = new GmailFolder(this.User, this.Pass))
            {
                var expenseMessages = new List<ExpenseMessage>();
                for (int i = 0; i < folder.Count; i++)
                {
                    var serverMsg = folder.GetMessage(i);
                    expenseMessages.Add(new ExpenseMessage()
                    {
                        Body = serverMsg.HtmlBody,
                        Subject = serverMsg.Subject,
                        Date = serverMsg.Date.LocalDateTime
                    });

                    folder.MarkRead(i);
                }

                var messageParser = new ExpenseMessageParser();
                expenses = messageParser.Parse(expenseMessages);
            }

            return expenses;
        }
    }
}
