using System;
using System.Globalization;
using System.Text.RegularExpressions;
using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Integrations.Files;

namespace ExpenseTracker.Allianz
{
    public class AllianzCsvParser : BaseCsvParser
    {
        protected override char FieldDelimiter => '|';

        protected override void ValidateHeader(string header)
        {
            // No header validation needed for Allianz CSV
        }

        protected override Transaction MapRowToEntity(string[] fields)
        {
            var reference = fields[1];
            var amount = GetAmount(fields);
            var type = fields[3] == "D" ? TransactionType.Expense : TransactionType.Income;
            var details = $"{fields[4]} {fields[5]} {fields[6]} {fields[7]} {fields[8]}".RemoveRepeatingSpaces();
            var parsedDate = ParseDate(details, fields[0]);

            return new Transaction
            {
                TransactionId = reference + "_" + amount.ToString("F2"),
                Amount = amount,
                Date = parsedDate,
                Details = details,
                Type = type,
                Source = "allianz_file"
            };
        }

        private static decimal GetAmount(string[] fields)
        {
            return decimal.Parse(fields[2].Replace(",", "").Replace(" ", ""));
        }

        private DateTime ParseDate(string details, string bankRecordDate)
        {
            var regex = new Regex(@"\d *\d *\. *\d *\d *\. *\d *\d *\d *\d");
            var date = regex.Match(details).Value.Replace(" ", "");
            regex = new Regex(@"\d *\d *: *\d *\d *: *\d *\d");
            var time = regex.Match(details).Value.Replace(" ", "");
            var parsedDate = default(DateTime);

            if (!string.IsNullOrEmpty(date))
            {
                parsedDate = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture);
                if (!string.IsNullOrEmpty(time))
                {
                    var parsedTime = TimeSpan.ParseExact(time, @"hh\:mm\:ss", CultureInfo.InvariantCulture);
                    parsedDate = new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, parsedTime.Hours, parsedTime.Minutes, parsedTime.Seconds);
                }
            }
            else
            {
                parsedDate = DateTime.ParseExact(bankRecordDate, "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
            }

            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Unspecified);
            if (parsedDate.TimeOfDay != default)
            {
                var bgTz = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
                var offset = bgTz.GetUtcOffset(parsedDate);
                parsedDate = parsedDate.Add(-offset);
                parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
            }

            return parsedDate;
        }
    }
}
