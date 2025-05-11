using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Services.Models;
using ExpenseTracker.Core.Transactions;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Collections.Generic;
using Telerik.JustMock;

namespace ExpenseTracker.Integrations.Tests
{
    [TestClass]
    public class MailImporterTests
    {
        [TestMethod]
        public void ImportsTransactionsCorrectly()
        {
            var dummyParser1 = Mock.Create<IExpenseMessageParser>();
            Mock.Arrange<ExpenseMessage, Transaction>(() => dummyParser1.Parse(Arg.IsAny<ExpenseMessage>())).Returns<ExpenseMessage>(x =>
            {
                if (x.Body == "dummy1")
                {
                    return new Transaction()
                    {
                        Details = "dummy1"
                    };
                }
                else
                {
                    return null;
                }
            });
            var dummyParser2 = Mock.Create<IExpenseMessageParser>();
            Mock.Arrange<ExpenseMessage, Transaction>(() => dummyParser2.Parse(Arg.IsAny<ExpenseMessage>())).Returns<ExpenseMessage>(x =>
            {
                if (x.Body == "dummy2")
                {
                    return new Transaction()
                    {
                        Details = "dummy2"
                    };
                }
                else
                {
                    return null;
                }
            });
            var mailClient = Mock.Create<IMailClient>();
            var msgList = new List<string>() { "any1", "dummy1", "other1", "dummy2" };
            Mock.Arrange(() => mailClient.GetMessage(Arg.AnyInt)).Returns<int>(index =>
            {
                return new ExpenseMessage() { Body = msgList[index] };
            });
            Mock.Arrange(() => mailClient.Delete(Arg.AnyInt)).DoInstead<int>(index =>
            {
                msgList.RemoveAt(index);
            });
            Mock.Arrange(() => mailClient.Count).Returns(() =>
            {
                return msgList.Count;
            });

            var dummyTransactionsService = Mock.Create<IExpensesService>();
            var transactionsList = new List<Transaction>();
            IEnumerable<CreateTransactionResult> skippedDummy = new CreateTransactionResult[0];
            Mock.Arrange(() => dummyTransactionsService.TryCreateTransactions(Arg.IsAny<IEnumerable<Transaction>>(), out skippedDummy)).DoInstead<IEnumerable<Transaction>>(x => transactionsList.AddRange(x));
            var memCache = Mock.Create<IMemoryCache>();
            var importer = new MailImporter(new IExpenseMessageParser[] { dummyParser1, dummyParser2 }, dummyTransactionsService, mailClient, memCache);
            importer.ImportTransactions(out IEnumerable<Transaction> ts, out IEnumerable<CreateTransactionResult> skipped);
            Assert.AreEqual(2, transactionsList.Count);
            Assert.AreEqual("dummy1", transactionsList[0].Details);
            Assert.AreEqual("dummy2", transactionsList[1].Details);
            Assert.AreEqual(2, msgList.Count);
            Assert.AreEqual("any1", msgList[0]);
            Assert.AreEqual("other1", msgList[1]);
            importer.ImportTransactions(out IEnumerable<Transaction> ts2, out IEnumerable<CreateTransactionResult> skipped2);
            Assert.AreEqual(2, transactionsList.Count);
            Assert.AreEqual("dummy1", transactionsList[0].Details);
            Assert.AreEqual("dummy2", transactionsList[1].Details);
            Assert.AreEqual(2, msgList.Count);
            Assert.AreEqual("any1", msgList[0]);
            Assert.AreEqual("other1", msgList[1]);
        }
    }
}