using System;
using System.Collections.Generic;
using System.Globalization;
using System.Text;

namespace ExpenseTracker.Core
{
    public static class TransactionExtensions
    {
        public static void GenerateTransactionId(this Transaction t)
        {
            t.TransactionId = $"{t.Date.ToString("dd/MM/yy_HH:mm:ss", CultureInfo.InvariantCulture)}_{t.Amount.ToString("F2")}_{t.Details}";
        }

        public static void GenerateTransactionId(this Transaction t, DateTime date)
        {
            t.TransactionId = $"{date.ToString("dd/MM/yy_HH:mm:ss", CultureInfo.InvariantCulture)}_{t.Amount.ToString("F2")}_{t.Details}";
        }
    }
}
