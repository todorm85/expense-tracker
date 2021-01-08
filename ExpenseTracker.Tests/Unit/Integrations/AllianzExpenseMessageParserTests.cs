using ExpenseTracker.Allianz;
using ExpenseTracker.Core;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Transactions.Rules;
using ExpenseTracker.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class AllianzExpenseMessageParserTests
    {
        private AllianzMessageFactory msgFactory = new AllianzMessageFactory();
        private AllianzExpenseMessageParser parser;

        [TestInitialize]
        public void Init()
        {
            this.parser = new AllianzExpenseMessageParser();
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
            this.msgFactory.Location += " test";
            var expense = this.parser.Parse(this.msgFactory.GetRandomValidMessage());
            Assert.AreEqual(decimal.Parse(this.msgFactory.Amount), expense.Amount);
            Assert.AreEqual(this.msgFactory.Date, expense.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual(this.msgFactory.Location.RemoveRepeatingSpaces(), expense.Details);
        }
    }
}