using ExpenseTracker.Allianz;
using ExpenseTracker.Core;
using ExpenseTracker.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Telerik.JustMock;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class AllianzExpenseMessageParserTests
    {
        private IEnumerable<Category> categories;
        private AllianzMessageFactory msgFactory = new AllianzMessageFactory();
        private AllianzExpenseMessageParser parser;

        [TestInitialize]
        public void Init()
        {
            this.parser = new AllianzExpenseMessageParser(new TransactionsService(Mock.Create<IUnitOfWork>(), Mock.Create<IBaseDataItemService<Category>>()));
        }

        [TestMethod]
        public void Parse_InValidExpenseMessage_NotParsed()
        {
            var expense = this.parser.Parse(this.msgFactory.GetMessage("Неуспешна картова транзакция"));
            Assert.IsTrue(expense == null);
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
    }
}