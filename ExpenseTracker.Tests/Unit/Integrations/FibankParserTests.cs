using ExpenseTracker.Allianz;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using Telerik.JustMock;
using ExpenseTracker.Integrations.Email.MessageParsers;

namespace ExpenseTracker.Integrations.Tests
{
    [TestClass]
    public class FibankParserTests
    {
        private const string Body = @"PLASHTANE NA POS (SHELL PODUENE SOFIA BG) BGN 55.91 S KARTA No 5***3846 (12.04.2023 10:45:14) RAZP.SUMA 5105.17 BGN.<!-- --><br />
<hr /><table width=""100%""><tr>
<td width=""50%"" style=""padding: 4px; font-size: 80%; text-align: justify; vertical-align: text-top;"">Този имейл и неговите прикачени файлове могат да съдържат поверителна и/или привилегирована информация. Ако не сте предвидения получател, моля, уведомете незабавно изпращача и изтрийте завинаги този имейл и неговите прикачени файлове, както и всички копия. Достъпът, копирането или разпространението на този имейл или неговите прикачени файлове или всякаква информация, съдържаща се в него, от което и да е друго лице, освен предвидения получател, е забранен от закона. Първа инвестиционна банка АД не носи отговорност за никаква комуникация от името на своите служители, включително гледни точки и мнения на автора на съобщението, което противоречи на етичните норми и добрите нрави на Банката. Първа инвестиционна банка АД не носи отговорност за правилното и пълно предаване на информацията, съдържаща се в настоящото съобщение. Този имейл и всички прикачени файлове се отварят на ваш собствен риск и Банката не поема отговорност за щети, причинени от компютърни вируси, прехвърлени от съобщението и неговите прикачени файлове, както и за каквито и да е последствия от неговото използване.</td>
<td width=""50%"" style=""padding: 4px; font-size: 80%; text-align: justify; vertical-align: text-top;"">This e-mail and its attachments may contain confidential and or privileged information. If you are not the intended recipient, please, notify the sender immediately and permanently delete this e-mail and its attachments, as well as any copies. Access, copying or distribution of this e-mail or its attachments, or any information contained therein, by any other person save its intended recipient, is prohibited by law. First Investment Bank AD is not liable for any communication on behalf of its employees, including views and opinions of the author of the message, which contradicts the Bank's ethical standards and good morals. First Investment Bank AD is not liable for the proper and complete transmission of the information contained in this communication. This e-mail and any attachments are opened at your own risk and the Bank does not assume liability for any damages caused by computer viruses transferred by the message and its attachments, as well as for any consequences by its use.</td>
</tr></table>";

        private const string Body2 = @"PLASHTANE NA POS (SHELL PODUENE SOFIA BG) BGN 55.91 S KARTA No 5***3846 (12.04.2023 10:45:15) RAZP.SUMA 5105.17 BGN.<!-- --><br />
<hr /><table width=""100%""><tr>
<td width=""50%"" style=""padding: 4px; font-size: 80%; text-align: justify; vertical-align: text-top;"">Този имейл и неговите прикачени файлове могат да съдържат поверителна и/или привилегирована информация. Ако не сте предвидения получател, моля, уведомете незабавно изпращача и изтрийте завинаги този имейл и неговите прикачени файлове, както и всички копия. Достъпът, копирането или разпространението на този имейл или неговите прикачени файлове или всякаква информация, съдържаща се в него, от което и да е друго лице, освен предвидения получател, е забранен от закона. Първа инвестиционна банка АД не носи отговорност за никаква комуникация от името на своите служители, включително гледни точки и мнения на автора на съобщението, което противоречи на етичните норми и добрите нрави на Банката. Първа инвестиционна банка АД не носи отговорност за правилното и пълно предаване на информацията, съдържаща се в настоящото съобщение. Този имейл и всички прикачени файлове се отварят на ваш собствен риск и Банката не поема отговорност за щети, причинени от компютърни вируси, прехвърлени от съобщението и неговите прикачени файлове, както и за каквито и да е последствия от неговото използване.</td>
<td width=""50%"" style=""padding: 4px; font-size: 80%; text-align: justify; vertical-align: text-top;"">This e-mail and its attachments may contain confidential and or privileged information. If you are not the intended recipient, please, notify the sender immediately and permanently delete this e-mail and its attachments, as well as any copies. Access, copying or distribution of this e-mail or its attachments, or any information contained therein, by any other person save its intended recipient, is prohibited by law. First Investment Bank AD is not liable for any communication on behalf of its employees, including views and opinions of the author of the message, which contradicts the Bank's ethical standards and good morals. First Investment Bank AD is not liable for the proper and complete transmission of the information contained in this communication. This e-mail and any attachments are opened at your own risk and the Bank does not assume liability for any damages caused by computer viruses transferred by the message and its attachments, as well as for any consequences by its use.</td>
</tr></table>";

        private const string Body3 = @"PLASHTANE NA POS (CHATGPT SUBSCRIPTION +14158799686 US) USD 24.3 (BGN 44.23) S KARTA No 5***3846 (02.04.2024 22:04:36) RAZP.SUMA 826.18 BGN.";

        [TestMethod]
        public void ValidMessageIsParsedCorrectly()
        {
            var parser = new FibankMessageParser();
            var result = parser.Parse(new ExpenseMessage()
            {
                Subject = "Fibank SMS and E-MAIL services",
                Body = Body
            });
            Assert.AreEqual(55.91M, result.Amount);
            Assert.AreEqual("SHELL PODUENE SOFIA BG", result.Details);
            Assert.AreEqual(new DateTime(2023, 4, 12,10, 45, 14), result.Date);
        }

        [TestMethod]
        public void ValidMessageIsParsedCorrectlyUSD()
        {
            var parser = new FibankMessageParser();
            var result = parser.Parse(new ExpenseMessage()
            {
                Subject = "Fibank SMS and E-MAIL services",
                Body = Body3
            });
            Assert.AreEqual(44.23M, result.Amount);
            Assert.AreEqual("CHATGPT SUBSCRIPTION +14158799686 US", result.Details);
            Assert.AreEqual(new DateTime(2024, 4, 2, 22, 4, 36), result.Date);
        }

        [TestMethod]
        public void ValidMessageUniqueness()
        {
            var parser = new FibankMessageParser();
            var result1 = parser.Parse(new ExpenseMessage()
            {
                Subject = "Fibank SMS and E-MAIL services",
                Body = Body
            });

            var result1_1 = parser.Parse(new ExpenseMessage()
            {
                Subject = "Fibank SMS and E-MAIL services",
                Body = Body
            });

            var result2 = parser.Parse(new ExpenseMessage()
            {
                Subject = "Fibank SMS and E-MAIL services",
                Body = Body2
            });

            Assert.AreEqual(result1.TransactionId, result1_1.TransactionId);
            Assert.AreNotEqual(result1.TransactionId, result2.TransactionId);
        }
    }
}