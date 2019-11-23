using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.AllianzTxtParser
{
    public class TxtFileParser
    {
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

                    t.Date = DateTime.ParseExact(fgs[0], "dd/MM/yyyy HH:mm:ss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal);
                    t.Date = t.Date.AddHours(-2); // Allianz exports to bg time zone without specifying timezone
                    t.TransactionId = fgs[1];
                    t.Amount = Decimal.Parse(fgs[2]);
                    t.Type = fgs[3] == "D" ? TransactionType.Expense : TransactionType.Income;
                    t.Source = $"{fgs[4]} {fgs[5]} {fgs[6]} {fgs[7]} {fgs[8]}".RemoveRepeatingSpaces();

                    trans.Add(t);

                    line = sr.ReadLine();
                }
            }

            return trans;
        }

        public IEnumerable<Transaction> GetTransactions(TransactionType type, string filePath)
        {
            var parser = new TxtFileParser();
            var ts = this.ParseFromFile(filePath).Where(t => t.Type == type);
            return ts;
        }

        public IEnumerable<Transaction> GetSalary(string filePath)
        {
            var income = this.GetTransactions(TransactionType.Income, filePath);
            return income.Where(x => x.Source.Contains("ЗАПЛАТА"));
        }
    }
}
