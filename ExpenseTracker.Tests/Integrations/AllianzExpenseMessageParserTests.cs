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
            var categoriesService = Mock.Create<IBaseDataItemService<Category>>();
            Mock.Arrange(() => categoriesService.GetAll()).Returns(() => this.categories);
            this.parser = new AllianzExpenseMessageParser(new TransactionBuilder(categoriesService));
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
            Assert.AreEqual(expense.Date.ToString("dd_MM_yy", CultureInfo.InvariantCulture) + "_" + expense.Amount, expense.TransactionId);
            Assert.AreEqual("cat1", expense.Category);
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
