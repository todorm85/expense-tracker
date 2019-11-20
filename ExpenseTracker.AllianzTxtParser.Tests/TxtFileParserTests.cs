using System;
using ExpenseTracker.Core;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.AllianzTxtParser.Tests
{
    [TestClass]
    public class TxtFileParserTests
    {
        private TxtFileParser sut = new TxtFileParser();

        [TestMethod]
        public void Parser_OneTransaction_ReturnsOne()
        {
            var data = @"datetime|reference|amount|dtkt|trname|contragent|rem_i|rem_ii|rem_iii
01/11/2019 09:12:29|161ADV4193050016|800.00|D|Теглене на АТМ в страната|424982***3480#RFB ATM 054203           SOF|IA        BG - В 09:17:00 на 31.1|0.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480|BG459115031";

            var res = this.sut.Parse(data);
            Assert.IsTrue(res.Count == 1, "Expected one transaction parsed");
            var t = res[0];
            Assert.IsTrue(t.Date == new DateTime(2019, 11, 1, 9, 12, 29));
            Assert.IsTrue(t.TransactionId == "161ADV4193050016");
            Assert.IsTrue(t.Amount == 800);
            Assert.IsTrue(t.Type == TransactionType.Expense);
            Assert.IsTrue(t.Source == "Теглене на АТМ в страната 424982***3480#RFB ATM 054203 SOF IA BG - В 09:17:00 на 31.1 0.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480 BG459115031");
        }

        [TestMethod]
        public void Parser_TwoTransaction_ReturnsTwo()
        {
            var data = @"datetime|reference|amount|dtkt|trname|contragent|rem_i|rem_ii|rem_iii
01/11/2019 09:12:29|161ADV4193050016|800.00|D|Теглене на АТМ в страната|424982***3480#RFB ATM 054203           SOF|IA        BG - В 09:17:00 на 31.1|0.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480|BG459115031
01/11/2019 19:12:29|161ADV4193050016|1.06|K||Такса теглене на АТМ в страната|||";

            var res = this.sut.Parse(data);
            Assert.IsTrue(res.Count == 2, "Expected one transaction parsed");

            var t1 = res[0];
            Assert.IsTrue(t1.Date == new DateTime(2019, 11, 1, 9, 12, 29));
            Assert.IsTrue(t1.TransactionId == "161ADV4193050016");
            Assert.IsTrue(t1.Amount == 800);
            Assert.IsTrue(t1.Type == TransactionType.Expense);
            Assert.IsTrue(t1.Source == "Теглене на АТМ в страната 424982***3480#RFB ATM 054203 SOF IA BG - В 09:17:00 на 31.1 0.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480 BG459115031");

            var t2 = res[1];
            Assert.IsTrue(t2.Date == new DateTime(2019, 11, 1, 19, 12, 29));
            Assert.IsTrue(t2.TransactionId == "161ADV4193050016");
            Assert.IsTrue(t2.Amount == (decimal)1.06);
            Assert.IsTrue(t2.Type == TransactionType.Income);
            Assert.IsTrue(t2.Source == "Такса теглене на АТМ в страната");
        }

        [TestMethod]
        [ExpectedException(typeof(FormatException))]
        public void Parser_DateIsOutOfRange_Throws()
        {
            var data = @"datetime|reference|amount|dtkt|trname|contragent|rem_i|rem_ii|rem_iii
01/11/2019 55:12:29|161ADV4193050016|800.00|D|Теглене на АТМ в страната|424982***3480#RFB ATM 054203           SOF|IA        BG - В 09:17:00 на 31.1|0.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480|BG459115031
01/11/2019 19:12:29|161ADV4193050016|1.06|K||Такса теглене на АТМ в страната|||";

            var res = this.sut.Parse(data);
            Assert.IsTrue(res.Count == 2, "Expected one transaction parsed");

            var t1 = res[0];
            Assert.IsTrue(t1.Date == new DateTime(2019, 11, 1, 9, 12, 29));
            Assert.IsTrue(t1.TransactionId == "161ADV4193050016");
            Assert.IsTrue(t1.Amount == 800);
            Assert.IsTrue(t1.Type == TransactionType.Expense);
            Assert.IsTrue(t1.Source == "Теглене на АТМ в страната 424982***3480#RFB ATM 054203 SOF IA BG - В 09:17:00 на 31.1 0.2019 Теглене АТМ-в мрежата на БОРИКА Райфайзенбанкжк Младост, бл. 30 София КОД : 001678 PAN*3480 BG459115031");

            var t2 = res[1];
            Assert.IsTrue(t2.Date == new DateTime(2019, 11, 1, 19, 12, 29));
            Assert.IsTrue(t2.TransactionId == "161ADV4193050016");
            Assert.IsTrue(t2.Amount == (decimal)1.06);
            Assert.IsTrue(t2.Type == TransactionType.Income);
            Assert.IsTrue(t2.Source == "Такса теглене на АТМ в страната");
        }
    }
}
