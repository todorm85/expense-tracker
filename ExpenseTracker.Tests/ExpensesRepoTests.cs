using System;
using System.Linq;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class ExpensesRepoTests
    {
        [TestInitialize]
        public void SetUp()
        {
            this.sut = new GenericRepo<Expense>(new LiteDB.LiteDatabase("tests.db"), "tests");
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Update_NonExistent_Throws()
        {
            var expenses = TestExpensesFactory.GetTestExpenses(1);
            expenses.First().Source = "PESHOOOOOOO";
            var initialCount = sut.GetAll().Count();
            expenses.First().Id = initialCount + 1;
            sut.Update(expenses);
        }

        [TestMethod]
        public void Update_Existing()
        {
            var expenses = TestExpensesFactory.GetTestExpenses(1);
            sut.Insert(expenses);
            expenses = sut.GetAll();
            var firstExpense = expenses.First();
            firstExpense.Source = "PESHOOOOOOO2121";
            sut.Update(new Expense[] { firstExpense });
            expenses = sut.GetAll();
            Assert.AreEqual("PESHOOOOOOO2121", expenses.First().Source);
        }

        private GenericRepo<Expense> sut;
    }
}