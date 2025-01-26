using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Integrations;
using ExpenseTracker.Integrations.Files;

namespace ExpenseTracker.Allianz
{
    public class AllianzCsvParser : TransactionCsvParserBase
    {
        protected override char FieldDelimiter => '|';

        protected override string FieldNameForDate => "datetime";
        protected override string FieldNameForAmount => "amount";
        protected override string[] FieldNamesForDetails => new[] { "trname", "contragent", "rem_i", "rem_ii", "rem_iii" };
        protected override IEnumerable<string> RequiredFields => base.RequiredFields.Concat(new string[] { "reference", "dtkt" });

        protected override Transaction MapRowToEntity(string[] fields)
        {
            var t = base.MapRowToEntity(fields);
            if (t == null)
                return null;

            if (TryGetFieldValue(fields, "dtkt", out string typeValue))
            {
                var type = typeValue == "D" ? TransactionType.Expense : TransactionType.Income;
                t.Type = type;

                return t;
            }

            return null;
        }

        protected override void SetTransactionId(Transaction t, string[] fields)
        {
            TryGetFieldValue(fields, "reference", out string reference);
            t.TransactionId = reference + "_" + t.Amount.ToString("F2");
        }

        protected override string ParseDetails(string[] fields)
        {
                return string.Join(" ", FieldNamesForDetails
                    .Select(fieldName => fields[Array.IndexOf(this.headerFields, fieldName)]))
                    .RemoveRepeatingSpaces();
        }

        protected override decimal ParseAmount(string[] fields)
        {
            return decimal.Parse(fields[2].Replace(",", "").Replace(" ", ""));
        }

        protected override DateTime ParseDate(string[] fields)
        {
            if (!TryGetFieldValue(fields, this.FieldNameForDate, out string bankRecordDate))
                throw new ArgumentException("Date field not found");
            
            var details = this.ParseDetails(fields);

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

            if (parsedDate.TimeOfDay != default)
            {
                return parsedDate.ConvertToUtcFromBgTimeUnknowKind();
            }
            else
            {
                return parsedDate;
            }
        }

        protected override string ParseSource(string[] fields)
        {
            return "allianz_file";
        }
    }
}
