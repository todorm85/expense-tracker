using System;
using System.Globalization;
using System.Text.RegularExpressions;
using ExpenseTracker.Allianz;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Integrations.Email.MessageParsers
{
    public class FibankMessageParser : IExpenseMessageParser
    {
        public Transaction Parse(ExpenseMessage expenseMessages)
        {
            Transaction t = null;
            if (expenseMessages.Subject.StartsWith("Fibank SMS and E-MAIL services"))
            {
                t = new Transaction();
                var rx = new Regex(@"^(?<operation>(TEGLENE OT ATM|PLASHTANE NA POS)) \((?<details>.+?)\) BGN (?<amount>[\d\.]+) S KARTA No 5\*+3846 \((?<date>.+?)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matches = rx.Matches(expenseMessages.Body);
                if (matches.Count == 0) return null;
                var op = matches[0].Groups["operation"].Value.Trim();
                t.Amount = decimal.Parse(matches[0].Groups["amount"].Value.Trim());
                t.Details = matches[0].Groups["details"].Value.Trim();
                if (op.StartsWith("TEGLENE"))
                    t.Details = op + " " + t.Details;
                var dateAndTime = matches[0].Groups["date"].Value.Trim();
                t.Date = DateTime.ParseExact(dateAndTime, "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                t.Type = TransactionType.Expense;
                t.Source = "fibank";
                t.TransactionId = $"{dateAndTime}_{t.Amount.ToString("F2")}";
            }

            return t;
        }
    }
}
