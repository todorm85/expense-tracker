using ExpenseTracker.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Xml;

namespace ExpenseTracker.Allianz
{
    public class RaiffeizenTxtFileParser
    {
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
                var typeRaw = entry.SelectSingleNode("d3p1:MovementType", m).InnerText;
                var type = typeRaw == "Debit" ? TransactionType.Expense : TransactionType.Income;
                var amount = decimal.Parse(entry.SelectSingleNode("d3p1:Amount", m).InnerText);
                var t = new Transaction() { Amount = amount, Type = type };
                var cardMovementDetails = entry.SelectSingleNode("d3p1:AccountCardMovementDetails", m);
                var documentMovementDetails = entry.SelectSingleNode("d3p1:MovementDocument", m);
                if (cardMovementDetails.InnerXml != string.Empty)
                {
                    var rawDate = cardMovementDetails.SelectSingleNode("d3p1:PostDate", m).InnerText;
                    t.Details = cardMovementDetails.SelectSingleNode("d3p1:Description", m).InnerText;
                    t.TransactionId = cardMovementDetails.SelectSingleNode("d3p1:DRN", m).InnerText;
                    t.Date = DateTime.ParseExact(rawDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                }
                else if (documentMovementDetails.InnerXml != string.Empty)
                {
                    var rawDate = documentMovementDetails.SelectSingleNode("d3p1:PaymentDate", m).InnerText;
                    t.Details = documentMovementDetails.SelectSingleNode("d3p1:Description", m).InnerText;
                    t.TransactionId = documentMovementDetails.SelectSingleNode("d3p1:DocumentReference", m).InnerText;
                    t.Date = DateTime.ParseExact(rawDate, "yyyy-MM-ddTHH:mm:ss.fff", CultureInfo.InvariantCulture);
                }
                else
                {
                    var rawDate = entry.SelectSingleNode("d3p1:ValueDate", m).InnerText;
                    t.Details = entry.SelectSingleNode("d3p1:Narrative", m).InnerText;
                    t.Date = DateTime.ParseExact(rawDate, "yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture);
                }

                t.Source = "reifeizen_file";
                trans.Add(t);
            }

            return trans;
        }

        public List<Transaction> ParseFile(string filePath)
        {
            XmlDocument document = new XmlDocument();
            document.Load(filePath);

            return this.Parse(document);
        }
    }
}