using System;
using System.Linq;
using ExpenseTracker.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class ExpensesRepoTests
    {
        [TestMethod]
        public void Insert()
        {
            var sut = new ExpensesRepo();
            var initialCount = sut.GetAll().Count();
            sut.Insert(TestExpensesFactory.GetTestExpenses(10));
            Assert.AreEqual(initialCount + 10, sut.GetAll().Count());
        }

        [TestMethod]
        [ExpectedException(typeof(InvalidOperationException))]
        public void Update_NonExistent_Throws()
        {
            var sut = new ExpensesRepo();
            var expenses = TestExpensesFactory.GetTestExpenses(1);
            expenses.First().Source = "PESHOOOOOOO";
            var initialCount = sut.GetAll().Count();
            expenses.First().Id = initialCount + 1;
            sut.Update(expenses);
        }

        [TestMethod]
        public void Update_Existing()
        {
            var sut = new ExpensesRepo();
            var expenses = TestExpensesFactory.GetTestExpenses(1);
            sut.Insert(expenses);
            expenses = sut.GetAll();
            var firstExpense = expenses.First();
            firstExpense.Source = "PESHOOOOOOO2121";
            sut.Update(firstExpense);
            expenses = sut.GetAll();
            Assert.AreEqual("PESHOOOOOOO2121", expenses.First().Source);
        }
    }
}