using System;
using System.Linq;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class GenericRepoTests
    {
        [TestInitialize]
        public void SetUp()
        {
            this.sut = new GenericRepo<Transaction>(new LiteDB.LiteDatabase("Filename=tests.db;utc=true;"), "tests");
        }

        [TestMethod]
        public void Update_NonExistent_DoesNothing()
        {
            var expenses = TestExpensesFactory.GetTestExpenses(1);
            var details = Guid.NewGuid().ToString();
            expenses.First().Source = details;
            var initialCount = sut.GetAll().Count();
            var newId = initialCount + 1;
            expenses.First().Id = newId;
            sut.Update(expenses);
            var expense = sut.GetAll().FirstOrDefault(x => x.Id == newId);
            Assert.IsNull(expense);
            expense = sut.GetAll().FirstOrDefault(x => x.Source == details);
            Assert.IsNull(expense);
        }

        [TestMethod]
        public void Update_Existing()
        {
            var expenses = TestExpensesFactory.GetTestExpenses(1);
            sut.Insert(expenses);
            expenses = sut.GetAll();
            var firstExpense = expenses.First();
            var details = Guid.NewGuid().ToString();
            firstExpense.Source = details;
            sut.Update(new Transaction[] { firstExpense });
            expenses = sut.GetAll();
            Assert.AreEqual(details, expenses.First().Source);
        }

        private GenericRepo<Transaction> sut;
    }
}