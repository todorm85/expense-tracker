using System.Collections.Generic;
using ExpenseTracker.Core;

namespace ExpenseTracker.GmailConnector
{
    public class ExpensesGmailImporter
    {
        private string user;
        private string pass;
        private ExpensesService service;

        public ExpensesGmailImporter(string user, string pass, ExpensesService service)
        {
            this.user = user;
            this.pass = pass;
            this.service = service;
        }

        public void Import()
        {
            IEnumerable<Expense> expenses;
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
                expenses = messageParser.Parse(expenseMessages);
            }

            service.Add(expenses);
        }
    }
}
