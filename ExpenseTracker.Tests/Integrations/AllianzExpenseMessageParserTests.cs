using System.Collections.Generic;
using System.Globalization;
using ExpenseTracker.Allianz;
using ExpenseTracker.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class AllianzExpenseMessageParserTests
    {
        private AllianzExpenseMessageParser parser;
        private TestMessageFactory msgFactory = new TestMessageFactory();
        private IEnumerable<Category> categories;

        [TestInitialize]
        public void Init()
        {
            this.parser = new AllianzExpenseMessageParser();
        }

        [TestMethod]
        public void Parse_ValidExpenseMessage_IsParsed()
        {
            this.categories = new Category[] { new Category() { Name = "cat1", KeyWord = "test" } };
            this.msgFactory.Details += " test";
            var expense = this.parser.Parse(this.msgFactory.GetMessage());
            Assert.AreEqual(decimal.Parse(this.msgFactory.Amount), expense.Amount);
            Assert.AreEqual(this.msgFactory.Date, expense.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual(this.msgFactory.Details.RemoveRepeatingSpaces(), expense.Details);
        }

        [TestMethod]
        public void Parse_InValidExpenseMessage_NotParsed()
        {
            this.msgFactory.Title = "Неуспешна картова транзакция";
            var expense = this.parser.Parse(this.msgFactory.GetMessage());
            Assert.IsTrue(expense == null);
        }
    }
}
