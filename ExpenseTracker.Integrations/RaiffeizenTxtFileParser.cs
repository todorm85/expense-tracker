using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml;
using ExpenseTracker.Core;

namespace ExpenseTracker.Allianz
{
    public class RaiffeizenTxtFileParser
    {
        private readonly ITransactionImporter builder;

        public RaiffeizenTxtFileParser(ITransactionImporter builder)
        {
            this.builder = builder;
        }

        public List<Transaction> ParseFile(string filePath)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            return this.Parse(document);
        }


        public List<Transaction> Parse(XmlDocument document)
        {
            XmlNamespaceManager m = new XmlNamespaceManager(document.NameTable);
            m.AddNamespace("d3p1", "http://schemas.datacontract.org/2004/07/DAIS.eBank.Client.WEB.UIFramework.Pages.Accounts.Models");
            m.AddNamespace("i", "http://www.w3.org/2001/XMLSchema-instance");
            m.AddNamespace("d2p1", "http://schemas.datacontract.org/2004/07/DAIS.eBank.Client.WEB.Internals.Common.Models.Filters");

            var trans = new List<Transaction>();


            var entries = document.SelectNodes("d3p1:Items/d3p1:AccountMovement", m);

            foreach (XmlNode entry in entries)
            {
                Transaction t = null;
                var movementDetails = entry.SelectSingleNode("d3p1:AccountCardMovementDetails", m);
                var typeRaw = entry.SelectSingleNode("d3p1:MovementType", m).InnerText;
                var type = typeRaw == "Debit" ? TransactionType.Expense : TransactionType.Income;
                var amount = Decimal.Parse(entry.SelectSingleNode("d3p1:Amount", m).InnerText);
                if (movementDetails.InnerXml != string.Empty)
                {
                    var details = movementDetails.SelectSingleNode("d3p1:Description", m).InnerText;
                    var transactionId = movementDetails.SelectSingleNode("d3p1:DRN", m).InnerText;
                    var rawDate = movementDetails.SelectSingleNode("d3p1:PostDate", m).InnerText;
                    var parsedDate = DateTime.ParseExact(rawDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                    t = this.builder.Import(amount, details, type, parsedDate);
                    t.TransactionId = transactionId;
                }
                else
                {
                    var details = entry.SelectSingleNode("d3p1:Narrative", m).InnerText;
                    var rawDate = entry.SelectSingleNode("d3p1:PaymentDate", m).InnerText;
                    var parsedDate = DateTime.ParseExact(rawDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                    t = this.builder.Import(amount, details, type, parsedDate);
                }

                if (IsValid(t))
                    trans.Add(t);
            }

            return trans;
        }

        private bool IsValid(Transaction t)
        {
            return t.Type == TransactionType.Expense;
        }
    }
}
