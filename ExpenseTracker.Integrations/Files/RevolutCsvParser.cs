using System;
using System.Globalization;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Integrations.Files
{
    // Derived class for Revolut CSV parsing
    public class RevolutCsvParser : TransactionCsvParserBase
    {
        protected override string FieldNameForDate => "Started Date";
        protected override string FieldNameForAmount => "Amount";
        protected override string[] FieldNamesForDetails => new[] { "Description" };

        protected override string ParseSource(string[] fields) => "revolut";

        protected override Transaction MapRowToEntity(string[] fields)
        {
            var transaction = base.MapRowToEntity(fields);
            if (transaction == null)
                return null;

            var revolutType = ParseRevolutType(fields);
            if (!string.IsNullOrEmpty(revolutType))
            {
                var tax = ParseFee(fields);
                if (revolutType == "TOPUP" || revolutType == "EXCHANGE")
                {
                    transaction.Amount = -tax;
                }
                else if (revolutType == "TRANSFER" || revolutType == "CARD_PAYMENT")
                {
                    transaction.Amount = transaction.Amount + (-tax);
                }
            }

            if (transaction.Amount != 0)
            {
                transaction.Type = transaction.Amount < 0 ? TransactionType.Expense : TransactionType.Income;
                transaction.Amount = Math.Abs(transaction.Amount);
                return transaction;
            }
            else
            {
                return null; // Ignore zero-amount transactions
            }
        }

        private string ParseRevolutType(string[] fields)
        {
            if (TryGetFieldValue(fields, "Type", out string revolutType))
            {
                return revolutType;
            }

            return null;
        }

        private decimal ParseFee(string[] fields)
        {
            if (TryGetFieldValue(fields, "Fee", out string taxValue))
            {
                return decimal.Parse(taxValue);
            }

            return 0;
        }

        protected override void SetTransactionId(Transaction t, string[] fields)
        {
            var revolutType = ParseRevolutType(fields);
            var amount = ParseAmount(fields);

            t.TransactionId = $"{revolutType}{t.Date.ToShortDateString()}{t.Date.ToLongTimeString()}{amount}{ParseFee(fields)}{t.Details}";
        }
    }
}
