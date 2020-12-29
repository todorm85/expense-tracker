using ExpenseTracker.Core;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ExpenseTracker.Allianz
{
    public class RaiffeisenMessageParser : IExpenseMessageParser
    {
        private readonly ITransactionImporter builder;

        public RaiffeisenMessageParser(ITransactionImporter builder)
        {
            this.builder = builder;
        }

        public Transaction Parse(ExpenseMessage expenseMessages)
        {
            var lines = expenseMessages.Body;
            Transaction t = null;
            if (lines.Contains("Bihme iskali da Vi uvedomim za POKUPKA"))
            {
                t = new Transaction();
                var rx = new Regex(@"Bihme iskali da Vi uvedomim za POKUPKA za (?<amount>[\d\.]+) BGN.+? pri (?<details>.+?) na (?<date>[\d\.]+?) .*", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matches = rx.Matches(lines);
                t.Amount = decimal.Parse(matches[0].Groups["amount"].Value.Trim());
                t.Details = matches[0].Groups["details"].Value.Trim();
                t.Date = DateTime.ParseExact(matches[0].Groups["date"].Value.Trim(), "dd.MM.yyyy", CultureInfo.InvariantCulture);
                return this.builder.Import(t.Amount, t.Details, t.Type, t.Date);
            }
            else
            {
                return null;
            }
        }
    }
}