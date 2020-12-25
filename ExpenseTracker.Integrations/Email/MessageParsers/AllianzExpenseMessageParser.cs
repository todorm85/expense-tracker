using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using ExpenseTracker.Core;

namespace ExpenseTracker.Allianz
{
    public class AllianzExpenseMessageParser : IExpenseMessageParser
    {
        private readonly ITransactionImporter builder;

        public AllianzExpenseMessageParser(ITransactionImporter builder)
        {
            this.builder = builder;
        }

        public Transaction Parse(ExpenseMessage message)
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
                    if (line.Contains(">Контрагент<"))
                        result.Details = this.GetSource(html);
                    if (line.Contains(">Сума<"))
                        result.Amount = this.GetAmount(html);

                    line = html.ReadLine();
                }
            }

            return this.builder.Import(result.Amount, result.Details, result.Type, result.Date);
        }

        private static bool IsValidExpenseMessage(ExpenseMessage message)
        {
            return message.Body.Contains("<title>Оторизирана картова транзакция</title>") ||
                message.Body.Contains("<title>Такса за поддръжка на сметка</title>") ||
                message.Body.Contains("<title>Такса издаване на карта</title>") ||
                message.Body.Contains("<title>Такса-проверка баланс,промяна ПИН</title>");
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

        private decimal GetAmount(StringReader reader)
        {
            SkipLines(reader, 2);
            var amountText = ExtractInnerText(reader.ReadLine()).Split(' ')[0];
            return (decimal)double.Parse(amountText);
        }
    }
}