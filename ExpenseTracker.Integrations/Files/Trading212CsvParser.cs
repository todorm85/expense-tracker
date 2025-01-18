using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Integrations.Files
{
    public class Trading212CsvParser : BaseCsvParser
    {
        private const string FieldNameForTransactionType = "Action";
        private const string FieldNameForDate = "Time";
        private const string FieldNameForNotes = "Notes";
        private const string FieldNameForMerchantName = "Merchant name";
        private const string FieldNameForCategory = "Merchant category";
        private const string FieldNameForTransactionId = "ID";
        private const string FieldNameForAmount = "Total";
        private const string FieldNameForAmountCurrency = "Currency (Total)";

        private const string FieldNameForAtmFees = "ATM Withdrawal Fee";


        private const string TransactionTypeCardDebit = "Card debit";

        private string[] headerFields;

        private static readonly string[] requiredFields = new[]
        {
                FieldNameForTransactionType,
                FieldNameForDate,
                FieldNameForNotes,
                FieldNameForMerchantName,
                FieldNameForCategory,
                FieldNameForTransactionId,
                FieldNameForAmount
            };

        protected override void ValidateHeader(string header)
        {
            this.headerFields = ParseFields(header);

            foreach (var field in requiredFields)
            {
                if (!this.headerFields.Contains(field))
                {
                    throw new ArgumentException($"Missing required field: {field}");
                }
            }
        }

        protected override Transaction MapRowToEntity(string[] fields)
        {
            var action = fields[Array.IndexOf(this.headerFields, FieldNameForTransactionType)];
            if (action != TransactionTypeCardDebit)
                return null;

            var date = ParseDate(fields[Array.IndexOf(this.headerFields, FieldNameForDate)]);
            var notes = fields[Array.IndexOf(this.headerFields, FieldNameForNotes)];
            var merchantName = fields[Array.IndexOf(this.headerFields, FieldNameForMerchantName)];
            var category = fields[Array.IndexOf(this.headerFields, FieldNameForCategory)];
            var details = string.Join(" - ", new[] { notes, merchantName, category }.Where(s => !string.IsNullOrEmpty(s)));
            var transactionId = fields[Array.IndexOf(this.headerFields, FieldNameForTransactionId)];
            var amount = decimal.Parse(fields[Array.IndexOf(this.headerFields, FieldNameForAmount)]);

            // Check for ATM fees and add to amount if available
            var atmFeeIndex = Array.IndexOf(this.headerFields, FieldNameForAtmFees);
            if (atmFeeIndex > 0 && !string.IsNullOrEmpty(fields[atmFeeIndex]))
            {
                var atmFee = decimal.Parse(fields[atmFeeIndex]);
                amount += atmFee;
            }

            // Check for currency and append to details if different from BGN
            var currencyIndex = Array.IndexOf(this.headerFields, FieldNameForAmountCurrency);
            if (currencyIndex > 0)
            {
                var currency = fields[currencyIndex];
                if (currency != "BGN")
                {
                    details = $"!!!!CURRENCY: {currency}. " + details;
                    // TODO: check for currency conversion fee. Currently there is no example in the test data.
                }
            }

            return new Transaction
            {
                TransactionId = transactionId,
                Date = date,
                Details = details,
                Amount = Math.Abs(amount),
                Type = amount < 0 ? Core.Transactions.TransactionType.Expense : Core.Transactions.TransactionType.Income,
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
