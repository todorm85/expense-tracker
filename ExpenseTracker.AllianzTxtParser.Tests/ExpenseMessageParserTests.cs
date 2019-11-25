﻿using System;
using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.Core;
using ExpenseTracker.GmailConnector;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class ExpenseMessageParserTests
    {
        private ExpenseMessageParser parser = new ExpenseMessageParser();
        private TestMessageFactory msgFactory = new TestMessageFactory();

        [TestMethod]
        public void Parse_ValidExpenseMessage_IsParsed()
        {
            var expenses = this.parser.Parse(new List<ExpenseMessage>() { msgFactory.GetMessage() });
            Assert.IsTrue(expenses.Count() > 0);
            var expense = expenses.First();
            Assert.AreEqual(decimal.Parse(msgFactory.Amount), expense.Amount);
            Assert.AreEqual(msgFactory.Date, expense.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual(msgFactory.Source.RemoveRepeatingSpaces(), expense.Details);
        }

        [TestMethod]
        public void Parse_InValidExpenseMessage_NotParsed()
        {
            msgFactory.Title = "Неуспешна картова транзакция";
            var expenses = this.parser.Parse(new List<ExpenseMessage>() { msgFactory.GetMessage() });
            Assert.IsTrue(expenses.Count() == 0);
        }
    }
}
