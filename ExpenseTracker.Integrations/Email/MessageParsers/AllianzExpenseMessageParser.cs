﻿using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ExpenseTracker.Allianz
{
    public class AllianzExpenseMessageParser : IExpenseMessageParser
    {
        public Transaction Parse(ExpenseMessage message)
        {
            if (!IsAllianzMessage(message))
                return null;
            if (!IsSuccessOperationMessage(message))
                return null;
            Transaction result = new Transaction();
            var tId = message.Subject.Split('#')[1].TrimEnd(')');
            result.TransactionId = tId;
            var title = string.Empty;
            var accountNumber = string.Empty;
            string reason = string.Empty;
            string actor = string.Empty;
            using (var html = new StringReader(message.Body))
            {
                var line = html.ReadLine();
                while (line != null)
                {
                    if (line.Contains("<title>"))
                        title = ExtractInnerText(line);
                    if (line.Contains(">Сметка<"))
                        accountNumber = ExtractInnerText(html.ReadLine());
                    if (line.Contains(">Дата<"))
                        result.Date = this.GetDate(html);
                    if (line.Contains(">Основание<"))
                        reason = this.GetReason(html);
                    if (line.Contains(">Контрагент<"))
                        actor = this.GetSource(html);
                    if (line.Contains(">Сума<"))
                        result.Amount = this.GetAmount(html);
                    line = html.ReadLine();
                }
            }

            result.Type = IsIncome(message) ? TransactionType.Income : TransactionType.Expense;
            SetDetails(result, title, accountNumber, reason, actor);
            result.Source = "allianz_mail";
            if (message.Body.RemoveRepeatingSpaces().Contains("Такса Нар.превод"))
                result.Details = "Такса Нар.превод";
            return result;
        }

        private void SetDetails(Transaction result, string title, string accountNumber, string reason, string actor)
        {
            reason = string.IsNullOrWhiteSpace(reason) ? string.Empty : $"_{reason}";
            result.Details = actor + reason;
            if (string.IsNullOrEmpty(result.Details))
                result.Details = title;

            result.Details = result.Details + $"_{accountNumber}";
        }

        private bool IsSuccessOperationMessage(ExpenseMessage message)
        {
            return !message.Body.Contains("Неуспешна картова транзакция");
        }

        private string ExtractInnerText(string line)
        {
            var rx = new Regex(@">(?<text>[^<>]+)<", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(line);
            if (matches.Count > 0)
                return matches[0].Groups["text"].Value.Trim();
            else
                return string.Empty;
        }

        private decimal GetAmount(StringReader reader)
        {
            SkipLines(reader, 2);
            var amountText = ExtractInnerText(reader.ReadLine()).Split(' ')[0];
            return (decimal)double.Parse(amountText.Replace(" ", ""));
        }

        private DateTime GetDate(StringReader reader)
        {
            var dateLine = reader.ReadLine();
            var date = ExtractInnerText(dateLine);
            DateTime result;
            if (DateTime.TryParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out result))
            {
                return DateTime.SpecifyKind(result, DateTimeKind.Utc);
            }
            else
            {
                throw new ArgumentException("The provided message body does not contain a date in the expected format.");
            }
        }

        private string GetReason(StringReader html)
        {
            return ExtractInnerText(html.ReadLine()).RemoveRepeatingSpaces().Trim();
        }

        private string GetSource(StringReader reader)
        {
            SkipLines(reader, 7);
            return reader.ReadLine().RemoveRepeatingSpaces();
        }

        private bool IsAllianzMessage(ExpenseMessage message)
        {
            return message.Subject.Contains("Движение по сметка: ");
        }

        private bool IsIncome(ExpenseMessage message)
        {
            return message.Body.Contains("Получен кредитен превод-IB") ||
                message.Body.Contains("Получен кр.превод") ||
                message.Body.Contains("Вноска по сметка") ||
                message.Body.Contains("Възстановени средства") ||
                message.Body.Contains("Получен превод");
        }

        private void SkipLines(StringReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                reader.ReadLine();
            }
        }
    }
}