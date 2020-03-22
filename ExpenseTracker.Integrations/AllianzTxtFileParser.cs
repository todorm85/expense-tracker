using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using ExpenseTracker.Core;

namespace ExpenseTracker.Allianz
{
    public class AllianzTxtFileParser
    {
        private readonly ITransactionBuilder builder;

        public AllianzTxtFileParser(ITransactionBuilder builder)
        {
            this.builder = builder;
        }

        public IEnumerable<Transaction> ParseFromFile(string filePath)
        {
            return this.Parse(File.ReadAllText(filePath));
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
                    var t = new Transaction();
                    var fgs = line.Split('|');

                    t.Amount = Decimal.Parse(fgs[2]);
                    t.Type = fgs[3] == "D" ? TransactionType.Expense : TransactionType.Income;
                    t.Details = $"{fgs[4]}{fgs[5]}{fgs[6]}{fgs[7]}{fgs[8]}".RemoveRepeatingSpaces();

                    var parsedDate = ParseDateFromDetails(t.Details);
                    if (parsedDate == default(DateTime))
                    {
                        // fallback to date when transaction has settled in bank
                        parsedDate = DateTime.ParseExact(fgs[0], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture);
                    }

                    parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
                    t.Date = parsedDate;
                    this.builder.Build(t);
                    trans.Add(t);

                    line = sr.ReadLine();
                }
            }

            return trans;
        }

        private DateTime ParseDateFromDetails(string source)
        {
            var regex = new Regex(@"\d\d.\d\d.\d\d\d\d");
            var date = regex.Match(source).Value;
            regex = new Regex(@"\d\d:\d\d:\d\d");
            var time = regex.Match(source).Value;
            if (string.IsNullOrEmpty(date))
            {
                return default(DateTime);
            }

            var parsedDate = DateTime.ParseExact(date, "dd.MM.yyyy", CultureInfo.InvariantCulture);
            if (!string.IsNullOrEmpty(time))
            {
                var parsedTime = TimeSpan.ParseExact(time, @"hh\:mm\:ss", CultureInfo.InvariantCulture);
                return new DateTime(parsedDate.Year, parsedDate.Month, parsedDate.Day, parsedTime.Hours, parsedTime.Minutes, parsedTime.Seconds);
            }
            else
            {
                return parsedDate;
            }
        }

        public IEnumerable<Transaction> GetTransactions(TransactionType type, string filePath)
        {
            var ts = this.ParseFromFile(filePath).Where(t => t.Type == type);
            return ts;
        }

        public IEnumerable<Transaction> GetSalary(string filePath)
        {
            var income = this.GetTransactions(TransactionType.Income, filePath);
            return income.Where(x => x.Details.Contains("ЗАПЛАТА"));
        }
    }
}
