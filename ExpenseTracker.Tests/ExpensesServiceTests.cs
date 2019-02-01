﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class ExpensesServiceTests
    {
        public TransactionsService Sut
        {
            get
            {
                if (this.sut == null)
                {
                    this.sut = new TransactionsService(this.uow);
                }

                return this.sut;
            }
        }

        [TestInitialize]
        public void Setup()
        {
            this.imptExpns = new List<Transaction>();

            this.repo = Mock.Create<IGenericRepository<Transaction>>(Behavior.Strict);
            this.uow = Mock.Create<IUnitOfWork>(Behavior.Strict);
            Mock.Arrange(() => this.uow.GetDataItemsRepo<Transaction>()).Returns(() => this.repo);
            this.sut = null;
        }

        [TestMethod]
        public void Export_NoTransactionsInRange_ExportsNothing()
        {
            var expeneses = new List<Transaction>
            {
                TestExpensesFactory.GetTestExpense(new DateTime(2018, 6, 1))
            };

            Mock.Arrange(() => this.repo.GetAll()).Returns(expeneses);

            var results = this.Sut.GetExpensesByCategoriesByMonths(new DateTime(2018, 1, 1), new DateTime(2018, 5, 1));

            Assert.AreEqual(0, results.Keys.Count);
        }

        [TestMethod]
        public void Export_SingleMonthExpensesInRange()
        {
            var expeneses = new List<Transaction>();
            var cat = "cat";
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 3, 1), cat));
            Mock.Arrange(() => this.repo.GetAll()).Returns(expeneses);

            var results = this.Sut.GetExpensesByCategoriesByMonths(new DateTime(2018, 1, 1), new DateTime(2018, 5, 1));

            var expectedKey = new DateTime(2018, 3, 1);
            Assert.AreEqual(1, results.Keys.Count);
            Assert.IsTrue(results.ContainsKey(expectedKey));
            Assert.IsTrue(results[expectedKey].Count() == 1);
            Assert.IsTrue(results[expectedKey][cat].First().Category == cat);
        }

        [TestMethod]
        public void Export_DifferentMonthExpensesInRange()
        {
            var expeneses = new List<Transaction>();
            var cat1 = "cat";
            var cat2 = "cat2";
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 3, 9), cat1));
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 4, 8), cat2));
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 4, 15), cat2));
            Mock.Arrange(() => this.repo.GetAll()).Returns(expeneses);

            var results = this.Sut.GetExpensesByCategoriesByMonths(new DateTime(2018, 1, 1), new DateTime(2018, 5, 1));

            Assert.AreEqual(2, results.Keys.Count);

            var expectedKey1 = new DateTime(2018, 3, 1);
            Assert.IsTrue(results.ContainsKey(expectedKey1));
            Assert.IsTrue(results[expectedKey1][cat1].Count() == 1);
            Assert.IsTrue(results[expectedKey1][cat1].First().Category == cat1);

            var expectedKey2 = new DateTime(2018, 4, 1);
            Assert.IsTrue(results.ContainsKey(expectedKey2));
            Assert.IsTrue(results[expectedKey2][cat2].Count() == 2);
            Assert.IsTrue(results[expectedKey2][cat2].First().Category == cat2);
            Assert.IsTrue(results[expectedKey2][cat2].Skip(1).First().Category == cat2);
        }

        [TestMethod]
        public void Export_NotDetailedDifferentMonthExpensesInRange()
        {
            var expeneses = new List<Transaction>();
            var cat1 = "cat";
            var cat2 = "cat2";
            var cat3 = "cat3";
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 3, 9), cat1));
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 4, 8), cat2));
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 4, 15), cat3));
            Mock.Arrange(() => this.repo.GetAll()).Returns(expeneses);

            var results = this.Sut.GetExpensesByCategoriesByMonths(new DateTime(2018, 1, 1), new DateTime(2018, 5, 1));

            Assert.AreEqual(2, results.Keys.Count);

            var expectedKey1 = new DateTime(2018, 3, 1);
            Assert.IsTrue(results.ContainsKey(expectedKey1));
            Assert.IsTrue(results[expectedKey1].Count() == 1);
            Assert.IsTrue(results[expectedKey1].First().Key == cat1);

            var expectedKey2 = new DateTime(2018, 4, 1);
            Assert.IsTrue(results.ContainsKey(expectedKey2));
            Assert.IsTrue(results[expectedKey2].Count() == 2);
            Assert.IsTrue(results[expectedKey2].First().Key == cat2);
            Assert.IsTrue(results[expectedKey2].Skip(1).First().Key == cat3);
        }
        
        private List<Transaction> imptExpns = new List<Transaction>();
        private IGenericRepository<Transaction> repo;
        private IUnitOfWork uow;
        private TransactionsService sut;
    }
}