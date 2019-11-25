using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using ExpenseTracker.Core;

namespace ExpenseTracker.GmailConnector
{
    public class ExpenseMessageParser
    {
        public IEnumerable<Transaction> Parse(List<ExpenseMessage> messages)
        {
            var result = new List<Transaction>();

            foreach (var message in messages)
            {
                var expense = this.Parse(message);
                if (expense != null)
                {
                    this.ValidateTransaction(expense);
                    result.Add(expense);
                }
            }

            return result;
        }

        private static bool IsValidExpenseMessage(ExpenseMessage message)
        {
            return message.Body.Contains("<title>Оторизирана картова транзакция</title>");
        }

        private static string ExtractInnerText(string line)
        {
            var rx = new Regex(@">(?<text>[^<>]+)<", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(line);
            return matches[0].Groups["text"].Value.Trim();
        }

        private static void SkipLines(StringReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                reader.ReadLine();
            }
        }

        private void ValidateTransaction(Transaction expense)
        {
            if (expense.Date == default(DateTime))
            {
                throw new InvalidDataException("The transaction could not have its date processed correctly.");
            }
        }

        private Transaction Parse(ExpenseMessage message)
        {
            if (!IsValidExpenseMessage(message))
            {
                return null;
            }

            Transaction result = new Transaction();

            using (var html = new StringReader(message.Body))
            {
                var line = html.ReadLine();
                while (line != null)
                {
                    if (line.Contains(">Дата<"))
                        result.Date = this.GetDate(html);
                    //if (line.Contains(">Сметка<"))
                    //    result.Account = this.GetAccount(html);
                    if (line.Contains(">Контрагент<"))
                        result.Details = this.GetSource(html);
                    if (line.Contains(">Сума<"))
                        result.Amount = this.GetAmount(html);

                    line = html.ReadLine();
                }
            }

            //result.TransactionId = this.GetTransactionId(message.Subject);
            //result.Date = message.EmailDate;

            return result;
        }

        private string GetSource(StringReader reader)
        {
            SkipLines(reader, 7);
            return reader.ReadLine().RemoveRepeatingSpaces();
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

        private string GetAccount(StringReader reader)
        {
            return ExtractInnerText(reader.ReadLine());
        }

        private decimal GetAmount(StringReader reader)
        {
            SkipLines(reader, 2);
            var amountText = ExtractInnerText(reader.ReadLine()).Split(' ')[0];
            return (decimal)double.Parse(amountText);
        }

        private string GetTransactionId(string line)
        {
            var rx = new Regex(@"\(#(?<text>\w+)\)", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(line);
            return matches[0].Groups["text"].Value.Trim();
        }
    }
}