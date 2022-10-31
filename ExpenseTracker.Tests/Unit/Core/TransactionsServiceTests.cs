using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Rules;
using ExpenseTracker.Data;
using LiteDB;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Linq;
using Telerik.JustMock;
using ExpenseTracker.Core.Services;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class TransactionsServiceTests
    {
        private List<Transaction> imptExpns = new List<Transaction>();

        private IRepository<Transaction> repo;

        private ExpensesService sut;

        private IUnitOfWork uow;

        public ExpensesService Sut
        {
            get
            {
                if (this.sut == null)
                {
                    this.sut = new ExpensesService(repo, Mock.Create<IRepository<Rule>>());
                }

                return this.sut;
            }
        }

        [TestMethod]
        [ExpectedException(typeof(LiteDB.LiteException))]
        public void Add_TransactionsWhenSameTransactionIdExists_Throws()
        {
            var expense = TestExpensesFactory.GetTestExpense(new DateTime(2018, 3, 9), "dummyCategory");
            expense.TransactionId = Guid.NewGuid().ToString();
            expense.Type = TransactionType.Expense;
            this.repo.Insert(new Transaction[] { expense });

            this.repo.Insert(new Transaction[] { expense });
            var results = this.Sut.GetAll();
            Assert.AreEqual(1, results.Count());
            var result = results.First();
            Assert.AreEqual(new DateTime(2018, 3, 9), result.Date);
        }

        [TestMethod]
        public void Add_TransactionsWithSameValuesButNoTransactionId_AddsBothEvenIfDuplicate()
        {
            var expense = TestExpensesFactory.GetTestExpense(new DateTime(2018, 3, 9), "dummyCategory");
            this.repo.Insert(new Transaction[] { expense });

            expense.TransactionId = "0";
            this.Sut.Insert(new Transaction[] { expense });
            var results = this.Sut.GetAll();
            Assert.AreEqual(2, results.Count());
            var result = results.First();
            Assert.AreEqual(new DateTime(2018, 3, 9), result.Date);
        }

        [TestInitialize]
        public void Setup()
        {
            this.imptExpns = new List<Transaction>();

            this.repo = new GenericRepo<Transaction>(new LiteDatabase("Filename=transactionsTests.db;utc=true;"), "Transactions");
            foreach (var item in this.repo.GetAll())
            {
                this.repo.RemoveById(item.TransactionId);
            }

            this.uow = Mock.Create<IUnitOfWork>(Behavior.Strict);
            Mock.Arrange(() => this.uow.GetDataItemsRepo<Transaction>()).Returns(() => this.repo);
            this.sut = null;
        }
    }
}