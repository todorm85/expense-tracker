using System;
using System.Globalization;

namespace ExpenseTracker.Core
{
    public static class TransactionExtensions
    {
        public static void GenerateTransactionId(this Transaction t)
        {
            t.GenerateTransactionId(t.Date);
        }

        public static void GenerateTransactionId(this Transaction t, DateTime date)
        {
            t.TransactionId = GenerateTransactionId(date, t.Amount, t.Details);
        }        

        public static string GenerateTransactionId(DateTime date, decimal amount, string details)
        {
            var detailsHash = Utils.ComputeCRC32Hash(details);
            return $"{date.ToString("dd.MM.yy.HH.mm.ss", CultureInfo.InvariantCulture)}_{amount.ToString("F2")}_{detailsHash}";
        }
    }
}