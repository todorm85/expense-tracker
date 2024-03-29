﻿using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Transactions;
using System;
using System.Globalization;
using System.Text.RegularExpressions;

namespace ExpenseTracker.Allianz
{
    public class RaiffeisenMessageParser : IExpenseMessageParser
    {
        public Transaction Parse(ExpenseMessage expenseMessages)
        {
            if (!IsReiffeisenMessage(expenseMessages))
                return null;
            var lines = expenseMessages.Body;
            Transaction t = null;
            if (IsPurchaseMessage(expenseMessages))
            {
                t = new Transaction();
                var rx = new Regex(@"(?<operation>(POKUPKA|TEGLENE NA ATM))\s{1}(za)*?\s+(?<amount>[\d\.]+)\s+BGN.*?pri\s+(?<details>(.|\s)+?)\sna\s(?<date>[\d.]+\s[\d:]+)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
                var matches = rx.Matches(lines);
                var op = matches[0].Groups["operation"].Value.Trim();
                t.Amount = decimal.Parse(matches[0].Groups["amount"].Value.Trim());
                t.Details = op + " " + matches[0].Groups["details"].Value.Trim();
                t.Date = DateTime.ParseExact(matches[0].Groups["date"].Value.Trim(), "dd.MM.yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                t.Type = TransactionType.Expense;
                t.Source = "reifeizen_mail";
                t.TransactionId = ExpensesService.GenerateTransactionId(t.Date, t.Amount, t.Details);
            }

            return t;
        }

        private bool IsPurchaseMessage(ExpenseMessage expenseMessages)
        {
            return expenseMessages.Body.Contains("Bihme iskali da Vi uvedomim za POKUPKA");
        }

        private bool IsReiffeisenMessage(ExpenseMessage expenseMessages)
        {
            return expenseMessages.Subject.Contains("Notification from RBBBG") ||
                expenseMessages.Subject.Contains("Notification CC RBB") ||
                (expenseMessages.From != null && expenseMessages.From.Contains("kbcinfo"));
        }
    }
}