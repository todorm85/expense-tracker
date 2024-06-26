﻿using ExpenseTracker.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;

namespace ExpenseTracker.Integrations.Files
{
    public class RevolutExcelParser
    {
        public List<Transaction> Parse(string data)
        {
            var trans = new List<Transaction>();

            using (var sr = new StringReader(data))
            {
                sr.ReadLine();
                var line = sr.ReadLine();
                while (!string.IsNullOrWhiteSpace(line))
                {
                    var delimiter = line.Where(x => x == ';').Count() == 10 ? ';' : ',';
                    var fgs = line.Split(delimiter);

                    if (fgs.Length != 10)
                    {
                        throw new Exception($"Unexpected count of columns for entry. {line}");
                    }

                    var revolutType = fgs[0];
                    var dateExecuted = ParseDate(fgs[2]);
                    var details = fgs[4];
                    var amount = decimal.Parse(fgs[5]);
                    var tax = decimal.Parse(fgs[6]);

                    var t = new Transaction()
                    {
                        TransactionId = $"{revolutType}{dateExecuted.ToShortDateString()}{dateExecuted.ToLongTimeString()}{amount}{tax}{details}",
                        Date = dateExecuted,
                        Details = details,
                        Source = "revolut"
                    };

                    if (revolutType == "TOPUP" || revolutType == "EXCHANGE")
                        t.Amount = -tax;
                    else if (revolutType == "TRANSFER" || revolutType == "CARD_PAYMENT")
                        t.Amount = amount + (-tax);

                    if (t.Amount != 0)
                    {
                        t.Type = t.Amount < 0 ? TransactionType.Expense : TransactionType.Income;
                        t.Amount = Math.Abs(t.Amount);
                        trans.Add(t);
                    }

                    line = sr.ReadLine();
                }
            }

            return trans;
        }

        public IEnumerable<Transaction> ParseFromFile(string filePath)
        {
            return this.Parse(File.ReadAllText(filePath));
        }

        private DateTime ParseDate(string date)
        {
            if (!DateTime.TryParseExact(date, "M/d/yyyy H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out DateTime parsedDate))
                if (!DateTime.TryParseExact(date, "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                    if (!DateTime.TryParseExact(date, "d.M.yyyy H:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out parsedDate))
                        throw new Exception("Invalid date format.");
            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Unspecified);
            var bgTz = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
            var offset = bgTz.GetUtcOffset(parsedDate);
            parsedDate = parsedDate.Add(-offset);
            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);

            return parsedDate;
        }
    }
}
