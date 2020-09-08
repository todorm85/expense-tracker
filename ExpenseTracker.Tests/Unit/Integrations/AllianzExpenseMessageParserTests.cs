using System.Collections.Generic;
using System.Globalization;
using ExpenseTracker.Allianz;
using ExpenseTracker.Core;
using ExpenseTracker.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class AllianzExpenseMessageParserTests
    {
        private AllianzExpenseMessageParser parser;
        private AllianzMessageFactory msgFactory = new AllianzMessageFactory();
        private IEnumerable<Category> categories;

        [TestInitialize]
        public void Init()
        {
            this.parser = new AllianzExpenseMessageParser(new TransactionImporter(Mock.Create<IBaseDataItemService<Category>>()));
        }

        [TestMethod]
        public void Parse_ValidExpenseMessage_IsParsed()
        {
            this.categories = new Category[] { new Category() { Name = "cat1", KeyWord = "test" } };
            this.msgFactory.Location += " test";
            var expense = this.parser.Parse(this.msgFactory.GetRandomValidMessage());
            Assert.AreEqual(decimal.Parse(this.msgFactory.Amount), expense.Amount);
            Assert.AreEqual(this.msgFactory.Date, expense.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual(this.msgFactory.Location.RemoveRepeatingSpaces(), expense.Details);
        }

        [TestMethod]
        public void Parse_InValidExpenseMessage_NotParsed()
        {
            var expense = this.parser.Parse(this.msgFactory.GetMessage("Неуспешна картова транзакция"));
            Assert.IsTrue(expense == null);
        }
    }
}
