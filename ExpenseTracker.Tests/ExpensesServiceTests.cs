using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class ExpensesServiceTests
    {
        private List<Expense> expensesFromCLient = new List<Expense>();
        private ExpensesRepo repo;
        private ExpensesService sut;
        private string testsPath = @"TestExpenses.db";

        [TestInitialize]
        public void Setup()
        {
            this.expensesFromCLient = new List<Expense>();

            var client = Mock.Create<IExpensesImporter>(Behavior.Strict);
            Mock.Arrange(() => client.Import()).Returns(() => this.expensesFromCLient);

            this.repo = new ExpensesRepo(testsPath);
            this.sut = new ExpensesService(client, repo);
        }

        [TestCleanup]
        public void Cleanup()
        {
            this.repo.Dispose();
            if (File.Exists(testsPath))
            {
                File.Delete(testsPath);
            }
        }

        [TestMethod]
        public void Import_ExistingTransactions_NotImported()
        {
            this.expensesFromCLient.Add(TestExpensesFactory.GetTestExpense());
            Assert.AreEqual(0, this.repo.GetAll().Count());
            this.sut.Import();
            this.sut.Import();
            Assert.AreEqual(1, this.repo.GetAll().Count());
            this.expensesFromCLient.Add(TestExpensesFactory.GetTestExpense());
            this.sut.Import();
            Assert.AreEqual(2, this.repo.GetAll().Count());
        }

        [TestMethod]
        public void Import_NoTransactions_NothingImported()
        {
            this.sut.Import();
            Assert.AreEqual(0, this.repo.GetAll().Count());
        }
    }
}
