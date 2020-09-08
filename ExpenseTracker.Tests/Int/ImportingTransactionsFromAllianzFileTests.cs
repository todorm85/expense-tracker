using System;
using System.Globalization;
using System.Linq;
using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.Core;
using ExpenseTracker.Tests.Common;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;

namespace ExpenseTracker.Tests.Int
{
    [TestClass]
    public class ImportingTransactionsFromAllianzFileTests : IntTestsBase
    {
        private AllianzTxtFileParser fileParser;
        private TransactionsService expensesService;
        private const string YouTubeExpenseEntry = @"01/09/2020 12:55:37|161ADV8202450006|10.99|D|Плащане чрез ПОС чужбина |424982***3046#GOOGLE *YouTubePremium   g.c|o/helppay#GB - g.co/helppay#GOOGL|E *YouTubePremium PAN*3046|BG459115043";
        private const string TaxEntry = @"02/09/2020 00:00:00|161CHMRBGNL00001|2.20|D|Такса за поддръжка на сметка||||";
        private const string SalaryEntry = @"31/08/2020 09:06:41|144FTBM202440255|4,322.81|K|Получен кр.превод-IB масово плащане|ПРОГРЕС СОФТУЕР  ЕАД|ЗАПЛАТА август 2020||BG97BUIN95611000360054";
        private const string EpayEntry = @"02/09/2020 12:56:58|161ADVF202460007|13.09|D|КТ Плащ.н.Visa Electr-н.POS търг.|56100025  #TID#424982***3046#EVN BALGARIA |          EASY PAY      BGR - В 2|1:50:38 на 01.09.2020 Плащане ПОС Easypay AD Ivan VazovSofia КОД : 000483 PAN*3046|BG09BUIN95611900260611";
        private const string FileHeader = "datetime|reference|amount|dtkt|trname|contragent|rem_i|rem_ii|rem_iii";
        private const string InternalTransaction1 = "05/08/2020 22:10:04|161PBWU202180001|400.00|D|Нар.кред.превод,e-BANK - БИСЕРА|Тодор Бориславов Мицковски|превод м/ъ собствени сметки||BG42RZBB91551011710480";
        private const string InternalTransaction2 = "10/08/2020 11:28:14|161FTWW202231002|400.00|D|Нареден кредитен превод-IB|ТОДОР БОРИСЛАВОВ МИЦКОВСКИ|превод между собствени сметки||BG24BUIN95611000591258";

        [TestInitialize]
        public override void Initialize()
        {
            base.Initialize();
            this.fileParser = container.Resolve<AllianzTxtFileParser>();
            this.expensesService = container.Resolve<TransactionsService>();
        }

        [TestCleanup]
        public override void CleanUp()
        {
            this.expensesService.Remove(this.expensesService.GetAll());
            base.CleanUp();
        }

        [TestMethod]
        public void DoesNotImportTwiceSameEntry()
        {
            var testData = GetTestData(YouTubeExpenseEntry);
            var expenses = this.fileParser.Parse(testData);
            this.expensesService.Add(expenses);
            this.expensesService.Add(expenses);

            Assert.AreEqual(1, this.expensesService.GetAll().Count());
        }

        [TestMethod]
        public void ImportsMultipleEntriesAndSkipsRepeating()
        {
            var testData = GetTestData(TaxEntry);
            var expenses = this.fileParser.Parse(testData);
            this.expensesService.Add(expenses);

            testData = GetTestData(YouTubeExpenseEntry, TaxEntry, EpayEntry);
            expenses = this.fileParser.Parse(testData);
            this.expensesService.Add(expenses);

            Assert.AreEqual(3, this.expensesService.GetAll().Count());
        }

        [TestMethod]
        public void ImportsDateFromSettledWhenNoneInDescription()
        {
            var testData = GetTestData(YouTubeExpenseEntry);
            var expenses = this.fileParser.Parse(testData);

            Assert.AreEqual(new DateTime(2020, 09, 01, 12, 55, 37).ToString(CultureInfo.InvariantCulture), expenses[0].Date.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void ImportsDateFromDetailsWhenNoneInDescription()
        {
            var testData = GetTestData(EpayEntry);
            var expenses = this.fileParser.Parse(testData);

            Assert.AreEqual(new DateTime(2020, 09, 01, 21, 50, 38).ToString(CultureInfo.InvariantCulture), expenses[0].Date.ToString(CultureInfo.InvariantCulture));
        }

        [TestMethod]
        public void ImportsSalaryCorrectly()
        {
            var testData = GetTestData(SalaryEntry);
            var expenses = this.fileParser.Parse(testData);

            Assert.AreEqual(TransactionType.Income, expenses[0].Type);
        }

        [TestMethod]
        public void DoesNotImportInternalTransaction()
        {
            var testData = GetTestData(SalaryEntry, InternalTransaction1, InternalTransaction2, TaxEntry);
            var expenses = this.fileParser.Parse(testData);
            this.expensesService.Add(expenses);

            Assert.AreEqual(2, this.expensesService.GetAll().Count());
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
