using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Services.Models;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Tests.Common;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Tests.Int
{
    [TestClass]
    public class ImportingTransactionsFromMailTests : IntTestsBase
    {
        private IReadRepository<Transaction> ExpensesRepo => expensesService as IReadRepository<Transaction>;

        private AllianzMessageFactory allianz = new AllianzMessageFactory();
        private IExpensesService expensesService;
        private RaiffeisenMsgFactory rai = new RaiffeisenMsgFactory();
        private MailImporter sut;

        [TestCleanup]
        public override void CleanUp()
        {
            this.sut.Dispose();
            base.CleanUp();
        }

        [TestMethod]
        public void ImportsCorrectlyWhenAllInValid()
        {
            this.mailClient.MockedMessages.Add(this.allianz.GetInValidMessage("25.06.2006", "55.35", "InvalidTest"));
            this.mailClient.MockedMessages.Add(this.rai.GetInValidMessage("25.06.2006", "55.35", "InvalidTest2"));
            this.mailClient.MockedMessages.Add(this.allianz.GetInValidMessage("25.06.2006", "55.35", "InvalidTest3"));
            this.sut.ImportTransactions(out IEnumerable<Transaction> ts, out IEnumerable<CreateTransactionResult> skip);
            IEnumerable<Transaction> expenses = this.ExpensesRepo.GetAll();
            Assert.AreEqual(0, expenses.Count());
            Assert.AreEqual(this.mailClient.MockedMessages.Count, 3);
            foreach (var msg in this.mailClient.MockedMessages)
            {
                Assert.IsTrue(msg.Body.Contains("InvalidTest"));
            }
        }

        [TestMethod]
        public void ImportsCorrectlyWhenOnlyOneInValid()
        {
            this.mailClient.MockedMessages.Add(this.allianz.GetInValidMessage("25.06.2006", "55.35", "InvalidTest"));
            this.sut.ImportTransactions(out IEnumerable<Transaction> ts, out IEnumerable<CreateTransactionResult> skip);
            var expenses = this.ExpensesRepo.GetAll();
            Assert.AreEqual(0, expenses.Count());
            Assert.AreEqual(this.mailClient.MockedMessages.Count, 1);
            foreach (var msg in this.mailClient.MockedMessages)
            {
                Assert.IsTrue(msg.Body.Contains("InvalidTest"));
            }
        }

        [TestMethod]
        public void ImportsCorrectlyWhenOnlyOneValid()
        {
            this.mailClient.MockedMessages.Add(this.allianz.GetValidMessage("25.06.2006", "55.35", "Bar"));
            this.sut.ImportTransactions(out IEnumerable<Transaction> ts, out IEnumerable<CreateTransactionResult> skip);
            var expenses = this.ExpensesRepo.GetAll();
            Assert.AreEqual(1, expenses.Count());
            var expense1 = expenses.First();
            Assert.AreEqual(55.35M, expense1.Amount);
            Assert.AreEqual("25.06.2006", expense1.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual("Bar_SDFSGDG8488TRSDRREE", expense1.Details);
            Assert.AreEqual(this.mailClient.MockedMessages.Count, 0);
        }

        [TestMethod]
        public void ImportsCorrectlyWhenOnlyValid()
        {
            this.mailClient.MockedMessages.Add(this.allianz.GetValidMessage("25.06.2006", "55.35", "Bar"));
            this.mailClient.MockedMessages.Add(this.rai.GetValidMessage("26.06.2007", "65.35", "Bar2"));
            this.sut.ImportTransactions(out IEnumerable<Transaction> ts, out IEnumerable<CreateTransactionResult> skip);
            var expenses = this.ExpensesRepo.GetAll().OrderBy(x => x.Amount);
            Assert.AreEqual(2, expenses.Count());
            var expense1 = expenses.First();
            var expense2 = expenses.Skip(1).Take(1).First();
            Assert.AreEqual(55.35M, expense1.Amount);
            Assert.AreEqual(65.35M, expense2.Amount);
            Assert.AreEqual("25.06.2006", expense1.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual("26.06.2007", expense2.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual("Bar_SDFSGDG8488TRSDRREE", expense1.Details);
            Assert.AreEqual("POKUPKA Bar2", expense2.Details);
            Assert.AreEqual(this.mailClient.MockedMessages.Count, 0);
        }

        [TestMethod]
        public void ImportsCorrectlyWhenValidInMiddle()
        {
            this.mailClient.MockedMessages.Add(this.rai.GetInValidMessage("25.06.2006", "55.35", "InvalidTest"));
            this.mailClient.MockedMessages.Add(this.rai.GetUnparsableMessage());
            this.mailClient.MockedMessages.Add(this.allianz.GetValidMessage("25.06.2006", "55.35", "Bar"));
            this.mailClient.MockedMessages.Add(this.rai.GetInValidMessage("25.06.2006", "55.35", "InvalidTest2"));
            this.sut.ImportTransactions(out IEnumerable<Transaction> ts, out IEnumerable<CreateTransactionResult> skip);
            var expenses = this.ExpensesRepo.GetAll();
            Assert.AreEqual(1, expenses.Count());
            var expense1 = expenses.First();
            Assert.AreEqual(55.35M, expense1.Amount);
            Assert.AreEqual("25.06.2006", expense1.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual("Bar_SDFSGDG8488TRSDRREE", expense1.Details);
            Assert.AreEqual(this.mailClient.MockedMessages.Count, 3);
            foreach (var msg in this.mailClient.MockedMessages)
            {
                Assert.IsTrue(msg.Body.Contains("InvalidTest"));
            }
        }

        [TestMethod]
        public void ImportsCorrectlyWhenValidLast()
        {
            this.mailClient.MockedMessages.Add(this.rai.GetInValidMessage("25.06.2006", "55.35", "InvalidTest"));
            this.mailClient.MockedMessages.Add(this.allianz.GetValidMessage("25.06.2006", "55.35", "Bar"));
            this.sut.ImportTransactions(out IEnumerable<Transaction> ts, out IEnumerable<CreateTransactionResult> skip);
            var expenses = this.ExpensesRepo.GetAll();
            Assert.AreEqual(1, expenses.Count());
            var expense1 = expenses.First();
            Assert.AreEqual(55.35M, expense1.Amount);
            Assert.AreEqual("25.06.2006", expense1.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual("Bar_SDFSGDG8488TRSDRREE", expense1.Details);
            Assert.AreEqual(this.mailClient.MockedMessages.Count, 1);
            foreach (var msg in this.mailClient.MockedMessages)
            {
                Assert.IsTrue(msg.Body.Contains("InvalidTest"));
            }
        }

        [TestMethod]
        public void ImportsOnlyValidAndSkipsInvalidOnesAndCleansOnlyValidFromServer()
        {
            this.mailClient.MockedMessages.Add(this.allianz.GetValidMessage("25.06.2006", "55.35", "Bar"));
            this.mailClient.MockedMessages.Add(this.allianz.GetInValidMessage("25.06.2006", "55.35", "InvalidTest"));
            this.mailClient.MockedMessages.Add(this.rai.GetInValidMessage("25.06.2006", "55.35", "InvalidTest2"));
            this.mailClient.MockedMessages.Add(this.rai.GetValidMessage("26.06.2007", "65.35", "Bar2"));
            this.mailClient.MockedMessages.Add(this.allianz.GetInValidMessage("25.06.2006", "55.35", "InvalidTest3"));
            this.mailClient.MockedMessages.Add(this.allianz.GetValidMessage("26.06.2008", "165.35", "Bar3"));
            this.mailClient.MockedMessages.Add(this.rai.GetValidMessage("26.06.2009", "265.35", "Bar4"));
            this.sut.ImportTransactions(out IEnumerable<Transaction> ts, out IEnumerable<CreateTransactionResult> skip);
            var expenses = this.ExpensesRepo.GetAll().OrderBy(x => x.Amount);
            Assert.AreEqual(4, expenses.Count());
            var expense1 = expenses.First();
            var expense2 = expenses.Skip(1).Take(1).First();
            var expense3 = expenses.Skip(2).Take(1).First();
            var expense4 = expenses.Skip(3).Take(1).First();
            Assert.AreEqual(55.35M, expense1.Amount);
            Assert.AreEqual(65.35M, expense2.Amount);
            Assert.AreEqual(165.35M, expense3.Amount);
            Assert.AreEqual(265.35M, expense4.Amount);
            Assert.AreEqual("25.06.2006", expense1.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual("26.06.2007", expense2.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual("26.06.2008", expense3.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual("26.06.2009", expense4.Date.ToString("dd.MM.yyyy"));
            Assert.AreEqual("Bar_SDFSGDG8488TRSDRREE", expense1.Details);
            Assert.AreEqual("POKUPKA Bar2", expense2.Details);
            Assert.AreEqual("Bar3_SDFSGDG8488TRSDRREE", expense3.Details);
            Assert.AreEqual("POKUPKA Bar4", expense4.Details);
            Assert.AreEqual(this.mailClient.MockedMessages.Count, 3);
            foreach (var msg in this.mailClient.MockedMessages)
            {
                Assert.IsTrue(msg.Body.Contains("InvalidTest"));
            }
        }

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            this.expensesService = serviceProvider.GetService<IExpensesService>();
            this.sut = new MailImporter(this.serviceProvider.GetServices<IExpenseMessageParser>().ToArray(),
                this.expensesService,
                this.serviceProvider.GetService<IMailClient>(),
                this.serviceProvider.GetService<IMemoryCache>());
        }
    }
}