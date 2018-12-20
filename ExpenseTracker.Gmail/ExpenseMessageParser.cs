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
        public IEnumerable<Expense> Parse(List<ExpenseMessage> messages)
        {
            var result = new List<Expense>();

            foreach (var message in messages)
            {
                var expense = this.Parse(message);
                if (expense != null)
                {
                    result.Add(expense);
                }
            }

            return result;
        }

        private Expense Parse(ExpenseMessage message)
        {
            if (!IsValidExpenseMessage(message))
            {
                return null;
            }

            Expense result = new Expense();

            using (var html = new StringReader(message.Body))
            {
                var line = html.ReadLine();
                while (line != null)
                {
                    ////if (line.Contains(">Дата<"))
                    ////    result.Date = this.GetDate(html);
                    if (line.Contains(">Сметка<"))
                        result.Account = this.GetAccount(html);
                    if (line.Contains(">Контрагент<"))
                        result.Source = this.GetSource(html);
                    if (line.Contains(">Сума<"))
                        result.Amount = this.GetAmount(html);

                    line = html.ReadLine();
                }
            }

            result.TransactionId = this.GetTransactionId(message.Subject);
            result.Date = message.Date;

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

        private string GetSource(StringReader reader)
        {
            SkipLines(reader, 7);
            return reader.ReadLine().Trim();
        }

        private DateTime GetDate(StringReader reader)
        {
            var dateLine = reader.ReadLine();
            var date = ExtractInnerText(dateLine);
            DateTime result;
            var formatProvider = CultureInfo.GetCultureInfo("bg-BG");
            if (DateTime.TryParse(date, formatProvider, DateTimeStyles.AssumeLocal, out result))
            {
                return result;
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