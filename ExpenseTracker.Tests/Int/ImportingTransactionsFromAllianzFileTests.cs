using ExpenseTracker.Allianz;
using ExpenseTracker.Core;
using ExpenseTracker.Core.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Unity;

namespace ExpenseTracker.Tests.Int
{
    [TestClass]
    public class ImportingTransactionsFromAllianzFileTests : IntTestsBase
    {
        private const string EpayEntry = @"02/09/2020 12:56:58|161ADVF202460007|13.09|D|КТ Плащ.н.Visa Electr-н.POS търг.|56100025  #TID#424982***3046#EVN BALGARIA |          EASY PAY      BGR - В 2|1:50:38 на 01.09.2020 Плащане ПОС Easypay AD Ivan VazovSofia КОД : 000483 PAN*3046|BG09BUIN95611900260611";
        private const string FileHeader = "datetime|reference|amount|dtkt|trname|contragent|rem_i|rem_ii|rem_iii";
        private const string TaxEntry = @"02/09/2020 00:00:00|161CHMRBGNL00001|2.20|D|Такса за поддръжка на сметка||||";
        private const string YouTubeExpenseEntry = @"01/09/2020 12:55:37|161ADV8202450006|10.99|D|Плащане чрез ПОС чужбина |424982***3046#GOOGLE *YouTubePremium   g.c|o/helppay#GB - g.co/helppay#GOOGL|E *YouTubePremium PAN*3046|BG459115043";
        private TransactionsService expensesService;
        private AllianzTxtFileParser fileParser;

        [TestCleanup]
        public override void CleanUp()
        {
            foreach (var item in this.expensesService.GetAll())
            {
                this.expensesService.RemoveById(item.TransactionId);
            }

            base.CleanUp();
        }

        [TestMethod]
        public void DoesNotImportTwiceSameEntry()
        {
            var testData = GetTestData(YouTubeExpenseEntry);
            var expenses = this.fileParser.Parse(testData);
            this.expensesService.TryAdd(expenses, out IEnumerable<TransactionInsertResult> res);
            this.expensesService.TryAdd(expenses, out IEnumerable<TransactionInsertResult> res2);

            Assert.AreEqual(1, this.expensesService.GetAll().Count());
        }

        [TestMethod]
        public void ImportsDateFromDetails()
        {
            var testData = GetTestData(EpayEntry);
            var expenses = this.fileParser.Parse(testData);

            Assert.AreEqual(new DateTime(2020, 09, 01, 18, 50, 38).ToString(CultureInfo.InvariantCulture), expenses[0].Date.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void ImportsDateFromSettledWhenNoneInDescription()
        {
            var testData = GetTestData(YouTubeExpenseEntry);
            var expenses = this.fileParser.Parse(testData);

            Assert.AreEqual(new DateTime(2020, 09, 01, 9, 55, 37).ToString(CultureInfo.InvariantCulture), expenses[0].Date.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void ImportsMultipleEntriesAndSkipsRepeating()
        {
            var testData = GetTestData(TaxEntry);
            var expenses = this.fileParser.Parse(testData);
            this.expensesService.TryAdd(expenses, out IEnumerable<TransactionInsertResult> res);

            testData = GetTestData(YouTubeExpenseEntry, TaxEntry, EpayEntry);
            expenses = this.fileParser.Parse(testData);
            this.expensesService.TryAdd(expenses, out IEnumerable<TransactionInsertResult> res1);

            Assert.AreEqual(3, this.expensesService.GetAll().Count());
        }

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            this.fileParser = container.Resolve<AllianzTxtFileParser>();
            this.expensesService = container.Resolve<TransactionsService>();
        }

        private string GetTestData(params string[] entries)
        {
            var result = FileHeader;
            foreach (var entry in entries)
            {
                result += Environment.NewLine + entry;
            }

            return result;
        }
    }
}