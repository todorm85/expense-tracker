using System;
using System.Globalization;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Integrations.Files
{
    // Derived class for Revolut CSV parsing
    public class RevolutCsvParser : BaseCsvParser
    {
        protected override void ValidateHeader(string header)
        {
            if (header != "Type,Product,Started Date,Completed Date,Description,Amount,Fee,Currency,State,Balance")
            {
                throw new ArgumentException("Unexpected header format. Must be: Type,Product,Started Date,Completed Date,Description,Amount,Fee,Currency,State,Balance");
            }
        }

        protected override Transaction MapRowToEntity(string[] fields)
        {
            var revolutType = fields[0];
            var dateExecuted = ParseDate(fields[2]);
            var details = fields[4];
            var amount = decimal.Parse(fields[5]);
            var tax = decimal.Parse(fields[6]);

            var transaction = new Transaction
            {
                TransactionId = $"{revolutType}{dateExecuted.ToShortDateString()}{dateExecuted.ToLongTimeString()}{amount}{tax}{details}",
                Date = dateExecuted,
                Details = details,
                Source = "revolut"
            };

            if (revolutType == "TOPUP" || revolutType == "EXCHANGE")
            {
                transaction.Amount = -tax;
            }
            else if (revolutType == "TRANSFER" || revolutType == "CARD_PAYMENT")
            {
                transaction.Amount = amount + (-tax);
            }

            if (transaction.Amount != 0)
            {
                transaction.Type = transaction.Amount < 0 ? TransactionType.Expense : TransactionType.Income;
                transaction.Amount = Math.Abs(transaction.Amount);
                return transaction;
            }

            return null; // Ignore zero-amount transactions
        }

        private DateTime ParseDate(string date)
        {
            var formats = new[]
            {
                "M/d/yyyy H:mm",
                "yyyy-MM-dd HH:mm:ss",
                "d.M.yyyy H:mm"
            };

            if (!DateTime.TryParseExact(date, formats, CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
            {
                throw new Exception("Invalid date format.");
            }

            return parsedDate.ConvertToUtcFromBgTimeUnknowKind();
        }
    }
}
