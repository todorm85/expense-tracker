using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.Core;
using MailKit;

namespace ExpenseTracker.GmailConnector
{
    public class ExpensesMailClient : IExpensesImporter
    {
        private string user;
        private string pass;

        public ExpensesMailClient(string user, string pass)
        {
            this.user = user;
            this.pass = pass;
        }

        public IEnumerable<Expense> Import()
        {
            using (var folder = new GmailFolder(this.user, this.pass))
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
                var expenses = messageParser.Parse(expenseMessages);

                return expenses;
            }
        }
    }
}
