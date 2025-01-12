using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Integrations.Files
{
    public class Trading212CsvParser : BaseCsvParser
    {
        protected override void ValidateHeader(string header)
        {
            if (header != "Action,Time,Notes,ID,Total,Currency (Total),Merchant name,Merchant category,ATM Withdrawal Fee")
            {
                throw new ArgumentException("Unexpected header format. Must be: Action,Time,Notes,ID,Total,Currency (Total),Merchant name,Merchant category,ATM Withdrawal Fee");
            }
        }

        protected override Transaction MapRowToEntity(string[] fields)
        {
            var action = fields[0];
            if (action != "Card debit")
            {
                return null; // Skip rows that are not "Card debit"
            }

            var date = ParseDate(fields[1]);
            var notes = fields[2];
            var merchantName = fields[6];
            var category = fields[7];
            var details = string.Join(" - ", new[] { notes, merchantName, category }.Where(s => !string.IsNullOrEmpty(s)));
            var transactionId = fields[3];
            var amount = decimal.Parse(fields[4]);

            return new Transaction
            {
                TransactionId = transactionId,
                Date = date,
                Details = details,
                Amount = Math.Abs(amount),
                Type = amount < 0 ? TransactionType.Expense : TransactionType.Income,
                Source = "Trading212"
            };
        }

        private DateTime ParseDate(string date)
        {
            var formats = new[]
            {
                "yyyy-MM-dd HH:mm:ss.fff",
                "yyyy-MM-dd HH:mm:ss",
                "dd/MM/yyyy HH:mm:ss",
                "MM/dd/yyyy HH:mm:ss"
            };

            if (!DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                throw new Exception("Invalid date format.");
            }

            return parsedDate.ConvertToUtcFromBgTimeUnknowKind();
        }
    }
}
