using ExpenseTracker.Core;
using ExpenseTracker.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Linq;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class MailImporterTests
    {
        private GenericRepo<Transaction> sut;

        [TestInitialize]
        public void SetUp()
        {
            this.sut = new GenericRepo<Transaction>(new LiteDB.LiteDatabase("Filename=tests.db;utc=true;"), "tests");
        }

        [TestMethod]
        public void Update_Existing()
        {
            var expenses = TestExpensesFactory.GetTestExpenses(1);
            sut.Insert(expenses);
            expenses = sut.GetAll();
            var firstExpense = expenses.First();
            var details = Guid.NewGuid().ToString();
            firstExpense.Details = details;
            sut.Update(new Transaction[] { firstExpense });
            expenses = sut.GetAll();
            Assert.AreEqual(details, expenses.First().Details);
        }

        [TestMethod]
        public void Update_NonExistent_DoesNothing()
        {
            var expenses = TestExpensesFactory.GetTestExpenses(1);
            var details = Guid.NewGuid().ToString();
            expenses.First().Details = details;
            var initialCount = sut.GetAll().Count();
            var newId = initialCount + 1;
            expenses.First().TransactionId = "newId";
            sut.Update(expenses);
            var expense = sut.GetAll().FirstOrDefault(x => x.TransactionId == "newId");
            Assert.IsNull(expense);
            expense = sut.GetAll().FirstOrDefault(x => x.Details == details);
            Assert.IsNull(expense);
        }
    }
}