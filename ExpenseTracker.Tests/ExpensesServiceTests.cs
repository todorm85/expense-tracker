using System;
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
        public ExpensesService Sut
        {
            get
            {
                if (this.sut == null)
                {
                    this.sut = new ExpensesService(new IExpensesImporter[] { this.importer }, new IExpensesExporter[] { this.exporter }, this.repo);
                }

                return this.sut;
            }
        }

        [TestInitialize]
        public void Setup()
        {
            this.imptExpns = new List<Expense>();

            this.importer = Mock.Create<IExpensesImporter>(Behavior.Strict);
            this.exporter = Mock.Create<IExpensesExporter>(Behavior.Strict);
            Mock.Arrange(() => this.importer.Import()).Returns(() => this.imptExpns);

            this.repo = Mock.Create<IExpensesRepository>(Behavior.Strict);
            this.sut = null;
        }

        [TestMethod]
        public void Import_ExistingTransactions_NotImported()
        {
            var repo = new ExpensesRepo(this.testsPath);
            this.repo = repo;
            try
            {
                this.imptExpns.Add(TestExpensesFactory.GetTestExpense());
                Assert.AreEqual(0, this.repo.GetAll().Count());
                this.Sut.Import();
                this.Sut.Import();
                Assert.AreEqual(1, this.repo.GetAll().Count());
                this.imptExpns.Add(TestExpensesFactory.GetTestExpense());
                this.Sut.Import();
                Assert.AreEqual(2, this.repo.GetAll().Count());
            }
            finally
            {
                repo.Dispose();
                if (File.Exists(this.testsPath))
                {
                    File.Delete(this.testsPath);
                }
            }
        }

        [TestMethod]
        public void Import_NoTransactions_NothingImported()
        {
            var repo = new ExpensesRepo(this.testsPath);
            this.repo = repo;
            try
            {
                this.Sut.Import();
                Assert.AreEqual(0, this.repo.GetAll().Count());
            }
            finally
            {
                repo.Dispose();
                if (File.Exists(this.testsPath))
                {
                    File.Delete(this.testsPath);
                }
            }
        }

        [TestMethod]
        public void Export_NoTransactionsInRange_ExportsNothing()
        {
            var expeneses = new List<Expense>();
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 6, 1)));
            Mock.Arrange(() => this.repo.GetAll()).Returns(expeneses);

            var results = this.ExecuteDetailedExport(new DateTime(2018, 1, 1), new DateTime(2018, 5, 1));

            Assert.AreEqual(0, results.Keys.Count);
        }

        [TestMethod]
        public void Export_SingleMonthExpensesInRange()
        {
            var expeneses = new List<Expense>();
            var cat = "cat";
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 3, 1), cat));
            Mock.Arrange(() => this.repo.GetAll()).Returns(expeneses);

            var results = this.ExecuteDetailedExport(new DateTime(2018, 1, 1), new DateTime(2018, 5, 1));

            var expectedKey = new DateTime(2018, 3, 1);
            Assert.AreEqual(1, results.Keys.Count);
            Assert.IsTrue(results.ContainsKey(expectedKey));
            Assert.IsTrue(results[expectedKey].Count() == 1);
            Assert.IsTrue(results[expectedKey].First().Category == cat);
        }

        [TestMethod]
        public void Export_DifferentMonthExpensesInRange()
        {
            var expeneses = new List<Expense>();
            var cat1 = "cat";
            var cat2 = "cat2";
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 3, 9), cat1));
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 4, 8), cat2));
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 4, 15), cat2));
            Mock.Arrange(() => this.repo.GetAll()).Returns(expeneses);

            var results = this.ExecuteDetailedExport(new DateTime(2018, 1, 1), new DateTime(2018, 5, 1));

            Assert.AreEqual(2, results.Keys.Count);

            var expectedKey1 = new DateTime(2018, 3, 1);
            Assert.IsTrue(results.ContainsKey(expectedKey1));
            Assert.IsTrue(results[expectedKey1].Count() == 1);
            Assert.IsTrue(results[expectedKey1].First().Category == cat1);

            var expectedKey2 = new DateTime(2018, 4, 1);
            Assert.IsTrue(results.ContainsKey(expectedKey2));
            Assert.IsTrue(results[expectedKey2].Count() == 2);
            Assert.IsTrue(results[expectedKey2].First().Category == cat2);
            Assert.IsTrue(results[expectedKey2].Skip(1).First().Category == cat2);
        }

        [TestMethod]
        public void Export_NotDetailedDifferentMonthExpensesInRange()
        {
            var expeneses = new List<Expense>();
            var cat1 = "cat";
            var cat2 = "cat2";
            var cat3 = "cat3";
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 3, 9), cat1));
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 4, 8), cat2));
            expeneses.Add(TestExpensesFactory.GetTestExpense(new DateTime(2018, 4, 15), cat3));
            Mock.Arrange(() => this.repo.GetAll()).Returns(expeneses);

            var results = this.ExecuteNonDetailedExport(new DateTime(2018, 1, 1), new DateTime(2018, 5, 1));

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

        private Dictionary<DateTime, IEnumerable<Expense>> ExecuteDetailedExport(DateTime from, DateTime to)
        {
            var exportMock = Mock.Arrange(() => this.exporter.Export(Arg.IsAny<Dictionary<DateTime, IEnumerable<Expense>>>()));
            exportMock.MustBeCalled();
            var results = new Dictionary<DateTime, IEnumerable<Expense>>();
            exportMock.DoInstead<Dictionary<DateTime, IEnumerable<Expense>>>(x => results = x);

            this.Sut.ExportByMonths(from, to);

            return results;
        }

        private Dictionary<DateTime, Dictionary<string, decimal>> ExecuteNonDetailedExport(DateTime from, DateTime to)
        {
            var exportMock = Mock.Arrange(() => this.exporter.Export(Arg.IsAny<Dictionary<DateTime, Dictionary<string, decimal>>>()));
            exportMock.MustBeCalled();
            var results = new Dictionary<DateTime, Dictionary<string, decimal>>();
            exportMock.DoInstead<Dictionary<DateTime, Dictionary<string, decimal>>>(x => results = x);

            this.Sut.ExportByMonths(from, to, false);

            return results;
        }

        private List<Expense> imptExpns = new List<Expense>();
        private IExpensesImporter importer;
        private IExpensesRepository repo;
        private ExpensesService sut;
        private string testsPath = @"TestExpenses.db";
        private IExpensesExporter exporter;
    }
}