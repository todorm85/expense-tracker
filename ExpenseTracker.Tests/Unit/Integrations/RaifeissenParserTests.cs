using ExpenseTracker.Allianz;
using ExpenseTracker.Core;
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
            var importer = new TransactionsService(Mock.Create<IUnitOfWork>(), Mock.Create<IBaseDataItemService<Category>>());
            var parser = new RaiffeisenMessageParser(importer);
            var result = parser.Parse(new ExpenseMessage()
            {
                Body = @"Uvazhaemi g-ne/g-zho,
Bihme iskali da Vi uvedomim za POKUPKA za 6.30 BGN s Vashata null * ***3680 v BGR pri EDDYS na BAKE na 28.01.2020 09:34:34.Razpolagaema nalichnost po kartata 379.59 BGN.
S uvazhenie,
                Raiffeisenbank(Bulgaria) EAD
Sofia 1407,
                blvd Nikola I.Vaptzarov  55
070010000(VIVACOM)  1721(A1 i Telenor)"
            });
            Assert.AreEqual(6.30M, result.Amount);
            Assert.AreEqual("EDDYS na BAKE", result.Details);
            Assert.AreEqual(new DateTime(2020, 1, 28), result.Date);
        }
    }
}