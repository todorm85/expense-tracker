using System;
using ExpenseTracker.Core;
using ExpenseTracker.GmailConnector;

namespace ExpenseTracker.Tests
{
    internal class TestMessageFactory
    {
        public TestMessageFactory()
        {
            this.TransactionId = "20354987";
            this.Title = "Оторизирана картова транзакция";
            this.Date = "12/11/2000";
            this.Amount = "35.67";
            this.Account = "24BUIN95611000591358";
            this.Source = "FANTASTICO 42                >SOFIYA  BG";
        }

        public string TransactionId { get; set; }

        public string Title { get; set; }

        public string Date { get; set; }

        public string Amount { get; set; }

        public string Account { get; set; }

        public string Source { get; set; }

        public ExpenseMessage GetMessage()
        {
            return new ExpenseMessage()
            {
                Body = this.GetTestMessageBody(),
                Subject = $"Движение по сметка: 24BUIN95611000591258 (#{this.TransactionId})",
                Date = DateTime.Parse(this.Date)
        };
        }

        private string GetTestMessageBody()
        {
            return $@"<!DOCTYPE html PUBLIC ""-//W3C//DTD XHTML 1.0 Transitional//EN"" ""http://www.w3.org/TR/xhtml1/DTD/xhtml1-transitional.dtd"">
<html xmlns=""http://www.w3.org/1999/xhtml"" >
<head>
    <meta http-equiv=""Content-Type"" content=""text/html; charset=unicode"" />
    <title>{this.Title}</title>
</head>
<body>
    <img src=""logo_Allianz__BG.jpg"" />
    <table cellspacing=""0"" cellpadding=""2"" border=""0"" style=""border-collapse:collapse; width:400px;"">
    <tr>
        <td align=""center"" bgColor=""#316ac5"">
            <strong><font color=""white"">Оторизирана картова транзакция</font></strong>
        </td>
    </tr>
    <tr>
    <td>
    <table cellspacing=""0"" cellpadding=""2"" border=""0"" style=""border-collapse:collapse; border:solid 1px black"">
        <tr>
            <td><strong>Дата</strong></td>
            <td align=""center"" colspan=""3"" style=""width:100%""><strong>{this.Date}</strong></td>
        </tr>
        <tr>
            <td align=""left"">Вальор</td>
            <td align=""center"" colspan=""3"">12.12.2018</td>
        </tr>
        <tr>
            <td><strong>Сметка</strong></td>
            <td align=""center"" colspan=""3""><strong>{this.Account}</strong></td>
        </tr>
        <tr>
            <td bgColor=""lemonchiffon"" colSpan=""1"">
                <font color=""red""><strong>Сума</strong></font>
            </td>
            <td align=""center"" bgColor=""lemonchiffon"" colspan=""3"">
                <font color=""red""><strong>{this.Amount} BGN</strong></font>
            </td>
        </tr>
        <tr style=""font-size:smaller"">
            <td><strong>Основание</strong></td>
            <td align=""center"" colspan=""3""></td>
        </tr>
        <tr style=""font-size:smaller"">
            <td rowspan=""2"">
                <strong>Контрагент</strong>
            </td>
            <td align=""center"" colspan=""3"">
                /****3480
            </td>
        </tr>
        <tr style=""font-size:smaller"">
            <td align=""center""colspan=""3"">
{this.Source}
            </td>
        </tr>
    </table>
    </td>
    </tr>
    </table>
    <p />

    <!-- advertisement -->
    <p>

    </p>

    <p />

    <font color=""#0000FF"" size=""1"" face=""Arial, Helvetica, sans-serif"">
    ATTENTION:<br />
    The information in this electronic mail message is private and confidential, and only intended for the addressee. Should<br />
    you any disclosure, reproduction, distribution or use of this message is strictly prohibited. Please delete the message <br />
    without copying or opening it.<br />
    </font>

</body>
</html>
";
        }
    }
}