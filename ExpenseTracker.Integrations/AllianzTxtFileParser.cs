﻿using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;

namespace ExpenseTracker.Allianz
{
    public class AllianzTxtFileParser
    {
        private readonly ITransactionsService service;

        public AllianzTxtFileParser(ITransactionsService builder)
        {
            this.service = builder;
        }

        public List<Transaction> Parse(string data)
        {
            var trans = new List<Transaction>();

            using (var sr = new StringReader(data))
            {
                sr.ReadLine();
                var line = sr.ReadLine();
                while (!string.IsNullOrWhiteSpace(line))
                {
                    var fgs = line.Split('|');
                    var amount = decimal.Parse(fgs[2]);
                    var type = fgs[3] == "D" ? TransactionType.Expense : TransactionType.Income;
                    var details = $"{fgs[4]} {fgs[5]} {fgs[6]} {fgs[7]} {fgs[8]}".RemoveRepeatingSpaces();
                    var parsedDate = ParseDate(details, fgs[0]);
                    var t = new Transaction()
                    {
                        Amount = amount,
                        Date = parsedDate,
                        Details = details,
                        Type = type
                    };
                    if (!t.Details.Contains("собствени сметки"))
                        trans.Add(t);
                    line = sr.ReadLine();
                }
            }

            return trans;
        }

        public IEnumerable<Transaction> ParseFromFile(string filePath)
        {
            return this.Parse(File.ReadAllText(filePath));
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