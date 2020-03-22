﻿using System;
using System.Collections.Generic;
using ExpenseTracker.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Telerik.JustMock;

namespace ExpenseTracker.Allianz.Tests
{
    [TestClass]
    public class TxtFileParserTests
    {
        private AllianzTxtFileParser sut;
        private IEnumerable<Category> categories;

        [TestInitialize]
        public void Init()
        {
            var categoriesService = Mock.Create<IBaseDataItemService<Category>>();
            this.categories = new Category[0];
            Mock.Arrange(() => categoriesService.GetAll()).Returns(() => this.categories);
            this.sut = new AllianzTxtFileParser(new TransactionBuilder(categoriesService));
        }

        [TestMethod]
        public void Parser_OneTransaction_ReturnsOne()
        {
            var data = @"datetime|reference|amount|dtkt|trname|contragent|rem_i|rem_ii|rem_iii
01/11/2019 09:12:29|161ADV4193050016|800.00|D|Теглене на АТМ в страната|424982***3480#RFB ATM 054203     test      SOF|IA        BG - В 09:17:00 на 31.1|0.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480|BG459115031";
            this.categories = new Category[] { new Category() { Name = "cat1", KeyWord = "test" } };
            var res = this.sut.Parse(data);
            Assert.IsTrue(res.Count == 1, "Expected one transaction parsed");
            var t = res[0];
            Assert.IsTrue(t.Date == new DateTime(2019, 10, 31, 9, 17, 0));
            Assert.IsTrue(t.Date.Kind == DateTimeKind.Utc);
            Assert.IsTrue(t.Amount == 800);
            Assert.IsTrue(t.Type == TransactionType.Expense);
            Assert.IsTrue(t.Category == "cat1");
            Assert.IsTrue(t.Details == "Теглене на АТМ в страната424982***3480#RFB ATM 054203 test SOFIA BG - В 09:17:00 на 31.10.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480BG459115031");
            Assert.AreEqual("31_10_19_800.00", t.TransactionId);
        }

        [TestMethod]
        public void Parser_TwoTransactionAndOneIsMissingDateTime_ReturnsTwo()
        {
            var data = @"datetime|reference|amount|dtkt|trname|contragent|rem_i|rem_ii|rem_iii
01/11/2019 09:12:29|161ADV4193050016|800.00|D|Теглене на АТМ в страната|424982***3480#RFB ATM 054203           SOF|IA        BG - В 09:17:00 на 31.1|0.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480|BG459115031
01/11/2019 19:12:29|161ADV4193050016|1.06|K||Такса теглене на АТМ в страната|||";

            var res = this.sut.Parse(data);
            Assert.IsTrue(res.Count == 2, "Expected one transaction parsed");

            var t1 = res[0];
            Assert.IsTrue(t1.Date == new DateTime(2019, 10, 31, 9, 17, 0));
            Assert.IsTrue(t1.Amount == 800);
            Assert.IsTrue(t1.Type == TransactionType.Expense);
            Assert.IsTrue(t1.Details == "Теглене на АТМ в страната424982***3480#RFB ATM 054203 SOFIA BG - В 09:17:00 на 31.10.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480BG459115031");

            var t2 = res[1];
            Assert.IsTrue(t2.Date == new DateTime(2019, 11, 1, 19, 12, 29));
            Assert.IsTrue(t2.Amount == (decimal)1.06);
            Assert.IsTrue(t2.Type == TransactionType.Income);
            Assert.IsTrue(t2.Details == "Такса теглене на АТМ в страната");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Parser_DateIsOutOfRange_Throws()
        {
            var data = @"datetime|reference|amount|dtkt|trname|contragent|rem_i|rem_ii|rem_iii
01/11/2019 15:12:29|161ADV4193050016|800.00|D|Теглене на АТМ в страната|424982***3480#RFB ATM 054203           SOF|IA        BG - В 09:17:00 на 32.1|0.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480|BG459115031
01/11/2019 18:12:29|161ADV4193050016|1.06|K||Такса теглене на АТМ в страната|||";

            var res = this.sut.Parse(data);
        }

        [TestMethod]
        public void Parser_OnlyDateInDetails_UsesDateFromDetailsAndMidnightHour()
        {
            var data = @"datetime|reference|amount|dtkt|trname|contragent|rem_i|rem_ii|rem_iii
01/11/2019 15:12:29|161ADV4193050016|800.00|D|Теглене на АТМ в страната|424982***3480#RFB ATM 054203           SOF|IA        BG - В на 30.1|0.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480|BG459115031
01/11/2019 18:12:29|161ADV4193050016|1.06|K||Такса теглене на АТМ в страната|||";

            var res = this.sut.Parse(data);
            var t1 = res[0];
            Assert.IsTrue(t1.Date == new DateTime(2019, 10, 30));
        }

        [TestMethod]
        public void Parser_OnlyTimeInDetails_UsesEntryDateAndTime()
        {
            var data = @"datetime|reference|amount|dtkt|trname|contragent|rem_i|rem_ii|rem_iii
01/11/2019 15:12:29|161ADV4193050016|800.00|D|Теглене на АТМ в страната|424982***3480#RFB ATM 054203           SOF|IA        BG - В 09:17:00 на | Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480|BG459115031
01/11/2019 18:12:29|161ADV4193050016|1.06|K||Такса теглене на АТМ в страната|||";

            var res = this.sut.Parse(data);
            var t1 = res[0];
            Assert.IsTrue(t1.Date == new DateTime(2019, 11, 1, 15, 12, 29));
        }
    }
}