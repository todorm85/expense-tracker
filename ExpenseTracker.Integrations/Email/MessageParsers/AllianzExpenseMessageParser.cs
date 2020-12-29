using ExpenseTracker.Core;
using System;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

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

            var title = string.Empty;
            using (var html = new StringReader(message.Body))
            {
                var line = html.ReadLine();
                while (line != null)
                {
                    if (line.Contains("<title>"))
                        title = ExtractInnerText(line);
                    if (line.Contains(">Дата<"))
                        result.Date = this.GetDate(html);
                    if (line.Contains(">Контрагент<"))
                        result.Details = this.GetSource(html);
                    if (line.Contains(">Сума<"))
                        result.Amount = this.GetAmount(html);

                    line = html.ReadLine();
                }
            }

            result.Type = IsIncome(message) ? TransactionType.Income : TransactionType.Expense;
            if (string.IsNullOrEmpty(result.Details))
                result.Details = title;

            return this.builder.Import(result.Amount, result.Details, result.Type, result.Date);
        }

        private static string ExtractInnerText(string line)
        {
            var rx = new Regex(@">(?<text>[^<>]+)<", RegexOptions.Compiled | RegexOptions.IgnoreCase);
            var matches = rx.Matches(line);
            return matches[0].Groups["text"].Value.Trim();
        }

        private static bool IsIncome(ExpenseMessage message)
        {
            return message.Body.Contains("Получен кредитен превод-IB") ||
                message.Body.Contains("Вноска по сметка");
        }

        private static bool IsValidExpenseMessage(ExpenseMessage message)
        {
            return message.Body.Contains("<title>Оторизирана картова транзакция</title>") ||
                message.Body.Contains("<title>Такса за поддръжка на сметка</title>") ||
                message.Body.Contains("Такса издаване на  карта") ||
                message.Body.Contains("<title>Такса-проверка баланс,промяна ПИН</title>") ||
                message.Body.Contains("Такса  Нар.превод,IB - БИСЕРА: превод м/ъ собствени сметки") ||
                (message.Body.Contains("Получен кредитен превод-IB") && !message.Body.Contains("/94BUIN95611000529567")) ||
                message.Body.Contains("Вноска по сметка");
        }

        private static void SkipLines(StringReader reader, int count)
        {
            for (int i = 0; i < count; i++)
            {
                reader.ReadLine();
            }
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

        private string GetSource(StringReader reader)
        {
            SkipLines(reader, 7);
            return reader.ReadLine().RemoveRepeatingSpaces();
        }
    }
}