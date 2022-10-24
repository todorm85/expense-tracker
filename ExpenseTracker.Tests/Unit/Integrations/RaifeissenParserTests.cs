using ExpenseTracker.Allianz;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Transactions.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Telerik.JustMock;

namespace ExpenseTracker.Integrations.Tests
{
    [TestClass]
    public class RaifeissenParserTests
    {
        [TestMethod]
        public void ValidMessageIsParsedCorrectly()
        {
            var parser = new RaiffeisenMessageParser();
            var result = parser.Parse(new ExpenseMessage()
            {
                Subject = "Notification from RBBBG",
                Body = @"Uvazhaemi g-ne/g-zho,
Bihme iskali da Vi uvedomim za POKUPKA za 6.30 BGN s Vashata null * ***3680 v BGR pri EDDYS na BAKE na 28.01.2020 09:34:34.Razpolagaema nalichnost po kartata 379.59 BGN.
S uvazhenie,
                Raiffeisenbank(Bulgaria) EAD
Sofia 1407,
                blvd Nikola I.Vaptzarov  55
070010000(VIVACOM)  1721(A1 i Telenor)"
            });
            Assert.AreEqual(6.30M, result.Amount);
            Assert.AreEqual("POKUPKA EDDYS na BAKE", result.Details);
            Assert.AreEqual(new DateTime(2020, 1, 28,9, 34, 34), result.Date);
        }

        [TestMethod]
        public void ValidMessageV2IsParsedCorrectly()
        {
            var parser = new RaiffeisenMessageParser();
            var result = parser.Parse(new ExpenseMessage()
            {
                Subject = "Notification from RBBBG",
                Body = @"Uvazhaemi g-ne/g-zho,
Bihme iskali da Vi uvedomim za POKUPKA  6.30 BGN s Vashata null * ***3680 v BGR pri EDDYS na BAKE na 28.01.2020 09:34:34.Razpolagaema nalichnost po kartata 379.59 BGN.
S uvazhenie,
                Raiffeisenbank(Bulgaria) EAD
Sofia 1407,
                blvd Nikola I.Vaptzarov  55
070010000(VIVACOM)  1721(A1 i Telenor)"
            });
            Assert.AreEqual(6.30M, result.Amount);
            Assert.AreEqual("POKUPKA EDDYS na BAKE", result.Details);
            Assert.AreEqual(new DateTime(2020, 1, 28, 9, 34, 34), result.Date);
        }

        [TestMethod]
        public void ValidMessageV3IsParsedCorrectly()
        {
            var parser = new RaiffeisenMessageParser();
            var result = parser.Parse(new ExpenseMessage()
            {
                Subject = "Notification CC RBB",
                Body = @"Uvazhaemi g-ne/g-zho,
Bihme iskali da Vi uvedomim za POKUPKA  96.73 BGN s Vashata  ****5806 v BGR pri SHELL PODUENE na 10.04.2021 13:55:32. Razpolagaema nalichnost po kartata 1113.36 BGN.
S uvazhenie,
Raiffeisenbank (Bulgaria) EAD
Sofia 1407, blvd Nikola I.Vaptzarov  55
070010000 (VIVACOM)  1721 (A1 i Telenor)"
            });
            Assert.AreEqual(96.73M, result.Amount);
            Assert.AreEqual("POKUPKA SHELL PODUENE", result.Details);
            Assert.AreEqual(new DateTime(2021, 4, 10, 13, 55, 32), result.Date);
        }

        [TestMethod]
        public void ValidMessageV4IsParsedCorrectly()
        {
            var parser = new RaiffeisenMessageParser();
            var result = parser.Parse(new ExpenseMessage()
            {
                Subject = "Notification CC RBB",
                Body = @"Uvazhaemi g-ne/g-zho, Bihme iskali da Vi uvedomim za POKUPKA  3.99
BGN s Vashata karta ****6793 v IRL pri GOOGLE *Google Storage na 12.10.2022 16:39:28.
Razpolagaema nalichnost po kartata 1686.71 BGN.S uvazhenie, KBC Bank Bulgaria EAD Sofia 1407, blvd Nikola I.Vaptzarov  55 070010000 (VIVACOM)  1721 (A1 i Yettel)"
            });
            Assert.AreEqual(3.99M, result.Amount);
            Assert.AreEqual("POKUPKA GOOGLE *Google Storage", result.Details);
            Assert.AreEqual(new DateTime(2022, 10, 12, 16, 39, 28), result.Date);
        }
    }
}