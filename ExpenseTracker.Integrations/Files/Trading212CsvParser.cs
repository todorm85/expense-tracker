using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Transactions;
using ExpenseTracker.Core.Transactions;
using Transaction = ExpenseTracker.Core.Transactions.Transaction;

namespace ExpenseTracker.Integrations.Files
{
    public class Trading212CsvParser : TransactionCsvParserBase
    {
        private const string FieldNameForAtmFees = "ATM Withdrawal Fee";
        private const string FieldNameForAmountCurrency = "Currency (Total)";
        private const string FieldNameForTransactionId = "ID";
        private const string FieldNameForTransactionType = "Action";

        protected override string FieldNameForDate => "Time";
        protected override string FieldNameForAmount => "Total";
        protected override string[] FieldNamesForDetails => new[] { "Notes", "Merchant name", "Merchant category" };

        protected override IEnumerable<string> RequiredFields => base.RequiredFields
            .Append(FieldNameForTransactionId)
            .Append(FieldNameForTransactionType);

        protected override string ParseSource(string[] fields)
        {
            return "Trading212";
        }

        protected override Transaction MapRowToEntity(string[] fields)
        {
            var t = base.MapRowToEntity(fields);
            if (t == null)
                return null;

            if (TryGetFieldValue(fields, FieldNameForTransactionType, out string action))
                t.Details = $"{action}. {t.Details}";

            if (TryGetFieldValue(fields, FieldNameForTransactionId, out string id))
                t.TransactionId = id;

            // Check for ATM fees and add to amount if available
            if (TryGetFieldValue(fields, FieldNameForAtmFees, out string atmFeeValue) &&
            !string.IsNullOrEmpty(atmFeeValue))
            {
                var atmFee = decimal.Parse(atmFeeValue);
                t.Amount += atmFee;
            }

            t.Type = t.Amount < 0 ? TransactionType.Expense : TransactionType.Income;
            t.Amount = Math.Abs(t.Amount);

            // Check for currency and append to details if different from BGN
            if (TryGetFieldValue(fields, FieldNameForAmountCurrency, out string currency) && 
            !string.IsNullOrEmpty(currency) && currency != "BGN")
            {
                t.Details = $"!!!!CURRENCY: {currency}. " + t.Details;
                // TODO: check for currency conversion fee. Currently there is no example in the test data.
            }

            return t;
        }
    }
}
