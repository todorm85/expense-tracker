using ExpenseTracker.Allianz;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Core.Rules;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.Xml;
using Telerik.JustMock;

namespace ExpenseTracker.Tests.Unit.Integrations.RaiffeizenParser
{
    [TestClass]
    public class RaiffeizenParserTests
    {
        private RaiffeizenXmlFileParser parser;

        private string xmlRaw = @"<d3p1:Items xmlns:d3p1=""http://schemas.datacontract.org/2004/07/DAIS.eBank.Client.WEB.UIFramework.Pages.Accounts.Models"" xmlns:i=""http://www.w3.org/2001/XMLSchema-instance"" xmlns:d2p1=""http://schemas.datacontract.org/2004/07/DAIS.eBank.Client.WEB.Internals.Common.Models.Filters"">
  <d3p1:AccountMovement>
    <d3p1:Account>
      <d3p1:AccountCode>3915</d3p1:AccountCode>
      <d3p1:AccountNumber>1011710480</d3p1:AccountNumber>
      <d3p1:AccountProductType>Сметка - Международна дебитна карта</d3p1:AccountProductType>
      <d3p1:Balance>
        <d3p1:Account>
          <d3p1:AccountCode>0</d3p1:AccountCode>
          <d3p1:AccountNumber>1011710480</d3p1:AccountNumber>
          <d3p1:AccountProductType i:nil=""true"" />
          <d3p1:Balance i:nil=""true"" />
          <d3p1:BankClientID i:nil=""true"" />
          <d3p1:BankClientName i:nil=""true"" />
          <d3p1:CCY i:nil=""true"" />
          <d3p1:CCYFixRate>0</d3p1:CCYFixRate>
          <d3p1:CCYRate>0</d3p1:CCYRate>
          <d3p1:CreatePaymentDisabled>false</d3p1:CreatePaymentDisabled>
          <d3p1:IBAN>BG42RZBB91551011710480</d3p1:IBAN>
          <d3p1:ProductTypeName i:nil=""true"" />
          <d3p1:ShortCut>08</d3p1:ShortCut>
          <d3p1:ShortName i:nil=""true"" />
        </d3p1:Account>
        <d3p1:ActualBalance>418.24</d3p1:ActualBalance>
        <d3p1:AmountBeginDay>829.48</d3p1:AmountBeginDay>
        <d3p1:AvailableBalance>418.24</d3p1:AvailableBalance>
        <d3p1:BalanceDate>2020-09-08T00:00:00</d3p1:BalanceDate>
        <d3p1:Overdraft>0.00</d3p1:Overdraft>
        <d3p1:TotalBlockAmount>0</d3p1:TotalBlockAmount>
      </d3p1:Balance>
      <d3p1:BankClientID>A37111</d3p1:BankClientID>
      <d3p1:BankClientName>TODOR BORISLAVOV MITSKOVSKI</d3p1:BankClientName>
      <d3p1:CCY>BGN</d3p1:CCY>
      <d3p1:CCYFixRate>1</d3p1:CCYFixRate>
      <d3p1:CCYRate>1</d3p1:CCYRate>
      <d3p1:CreatePaymentDisabled>false</d3p1:CreatePaymentDisabled>
      <d3p1:IBAN>BG42RZBB91551011710480</d3p1:IBAN>
      <d3p1:ProductTypeName i:nil=""true"" />
      <d3p1:ShortCut>08</d3p1:ShortCut>
      <d3p1:ShortName>1011710480BGN</d3p1:ShortName>
    </d3p1:Account>
    <d3p1:AccountCardMovementDetails>
      <d3p1:Amount>163.5900000000</d3p1:Amount>
      <d3p1:AuthNumber i:nil=""true"" />
      <d3p1:CCY>BGN</d3p1:CCY>
      <d3p1:CardName i:nil=""true"" />
      <d3p1:CardNumber>5168********5806</d3p1:CardNumber>
      <d3p1:DRN>2232600989</d3p1:DRN>
      <d3p1:Description>Покупка/Purchase(POS) BILLA 404 YAMBOL/Yambol BGR</d3p1:Description>
      <d3p1:PostDate>2020-09-02T00:00:00</d3p1:PostDate>
    </d3p1:AccountCardMovementDetails>
    <d3p1:Amount>163.59</d3p1:Amount>
    <d3p1:AmountDisplayPurpose>danger</d3p1:AmountDisplayPurpose>
    <d3p1:AmountToShow>-163.59</d3p1:AmountToShow>
    <d3p1:CCY>BGN</d3p1:CCY>
    <d3p1:CreditTransferAmount>0</d3p1:CreditTransferAmount>
    <d3p1:DebitTransferAmount>163.59</d3p1:DebitTransferAmount>
    <d3p1:DocRegNumber>GE-GPI</d3p1:DocRegNumber>
    <d3p1:DocumentReference i:nil= ""true"" />
    <d3p1:DownloadMovementDocument>false</d3p1:DownloadMovementDocument>
    <d3p1:MovementDocument i:nil= ""true"" />
    <d3p1:MovementFunctionalType i:nil= ""true"" />
    <d3p1:MovementType>Debit</d3p1:MovementType>
    <d3p1:Narrative>Покупка/Purchase(POS) BILLA 404 YAMBOL/Yambol BGR</d3p1:Narrative>
    <d3p1:NarrativeAdditional i:nil= ""true"" />
    <d3p1:NarrativeForExports>Плащ.при РББГ търговец</d3p1:NarrativeForExports>
    <d3p1:OppositeSideAccount i:nil= ""true"" />
    <d3p1:OppositeSideName i:nil= ""true"" />
    <d3p1:OurSideAccount>BG42RZBB91551011710480</d3p1:OurSideAccount>
    <d3p1:OurSideName i:nil= ""true"" />
    <d3p1:PaymentDate>2020-09-03T00:00:00</d3p1:PaymentDate>
    <d3p1:Reason i:nil= ""true"" />
    <d3p1:ShowDocumentLink>false</d3p1:ShowDocumentLink>
    <d3p1:ValueDate>2020-09-02T00:00:00</d3p1:ValueDate>
  </d3p1:AccountMovement>
  <d3p1:AccountMovement>
    <d3p1:Account>
      <d3p1:AccountCode>3915</d3p1:AccountCode>
      <d3p1:AccountNumber>1011710480</d3p1:AccountNumber>
      <d3p1:AccountProductType>Сметка - Международна дебитна карта</d3p1:AccountProductType>
      <d3p1:Balance>
        <d3p1:Account>
          <d3p1:AccountCode>0</d3p1:AccountCode>
          <d3p1:AccountNumber>1011710480</d3p1:AccountNumber>
          <d3p1:AccountProductType i:nil= ""true"" />
          <d3p1:Balance i:nil= ""true"" />
          <d3p1:BankClientID i:nil= ""true"" />
          <d3p1:BankClientName i:nil= ""true"" />
          <d3p1:CCY i:nil= ""true"" />
          <d3p1:CCYFixRate>0</d3p1:CCYFixRate>
          <d3p1:CCYRate>0</d3p1:CCYRate>
          <d3p1:CreatePaymentDisabled>false</d3p1:CreatePaymentDisabled>
          <d3p1:IBAN>BG42RZBB91551011710480</d3p1:IBAN>
          <d3p1:ProductTypeName i:nil= ""true"" />
          <d3p1:ShortCut>08</d3p1:ShortCut>
          <d3p1:ShortName i:nil= ""true"" />
        </d3p1:Account>
        <d3p1:ActualBalance>418.24</d3p1:ActualBalance>
        <d3p1:AmountBeginDay>829.48</d3p1:AmountBeginDay>
        <d3p1:AvailableBalance>418.24</d3p1:AvailableBalance>
        <d3p1:BalanceDate>2020-09-08T00:00:00</d3p1:BalanceDate>
        <d3p1:Overdraft>0.00</d3p1:Overdraft>
        <d3p1:TotalBlockAmount>0</d3p1:TotalBlockAmount>
      </d3p1:Balance>
      <d3p1:BankClientID>A37111</d3p1:BankClientID>
      <d3p1:BankClientName>TODOR BORISLAVOV MITSKOVSKI</d3p1:BankClientName>
      <d3p1:CCY>BGN</d3p1:CCY>
      <d3p1:CCYFixRate>1</d3p1:CCYFixRate>
      <d3p1:CCYRate>1</d3p1:CCYRate>
      <d3p1:CreatePaymentDisabled>false</d3p1:CreatePaymentDisabled>
      <d3p1:IBAN>BG42RZBB91551011710480</d3p1:IBAN>
      <d3p1:ProductTypeName i:nil= ""true"" />
      <d3p1:ShortCut>08</d3p1:ShortCut>
      <d3p1:ShortName>1011710480BGN</d3p1:ShortName>
    </d3p1:Account>
    <d3p1:AccountCardMovementDetails>
      <d3p1:Amount>4.9400000000</d3p1:Amount>
      <d3p1:AuthNumber i:nil= ""true"" />
      <d3p1:CCY>BGN</d3p1:CCY>
      <d3p1:CardName i:nil= ""true"" />
      <d3p1:CardNumber>5168********5806</d3p1:CardNumber>
      <d3p1:DRN>2228098981</d3p1:DRN>
      <d3p1:Description>Покупка/Purchase(POS) GRAND MARKET/SVETI VLAS BGR</d3p1:Description>
      <d3p1:PostDate>2020-08-31T00:00:00</d3p1:PostDate>
    </d3p1:AccountCardMovementDetails>
    <d3p1:Amount>4.94</d3p1:Amount>
    <d3p1:AmountDisplayPurpose>danger</d3p1:AmountDisplayPurpose>
    <d3p1:AmountToShow>-4.94</d3p1:AmountToShow>
    <d3p1:CCY>BGN</d3p1:CCY>
    <d3p1:CreditTransferAmount>0</d3p1:CreditTransferAmount>
    <d3p1:DebitTransferAmount>4.94</d3p1:DebitTransferAmount>
    <d3p1:DocRegNumber>GE-GPI</d3p1:DocRegNumber>
    <d3p1:DocumentReference i:nil= ""true"" />
    <d3p1:DownloadMovementDocument>false</d3p1:DownloadMovementDocument>
    <d3p1:MovementDocument i:nil= ""true"" />
    <d3p1:MovementFunctionalType i:nil= ""true"" />
    <d3p1:MovementType>Debit</d3p1:MovementType>
    <d3p1:Narrative>Покупка/Purchase(POS) GRAND MARKET/SVETI VLAS BGR</d3p1:Narrative>
    <d3p1:NarrativeAdditional i:nil= ""true"" />
    <d3p1:NarrativeForExports>Плащане при БГ търговец</d3p1:NarrativeForExports>
    <d3p1:OppositeSideAccount i:nil= ""true"" />
    <d3p1:OppositeSideName i:nil= ""true"" />
    <d3p1:OurSideAccount>BG42RZBB91551011710480</d3p1:OurSideAccount>
    <d3p1:OurSideName i:nil= ""true"" />
    <d3p1:PaymentDate>2020-09-01T00:00:00</d3p1:PaymentDate>
    <d3p1:Reason i:nil= ""true"" />
    <d3p1:ShowDocumentLink>false</d3p1:ShowDocumentLink>
    <d3p1:ValueDate>2020-08-31T00:00:00</d3p1:ValueDate>
  </d3p1:AccountMovement>
</d3p1:Items>
";

        [TestInitialize]
        public void Init()
        {
            this.parser = new RaiffeizenXmlFileParser();
        }

        [TestMethod]
        public void Parse_InValidExpenseMessage_NotParsed()
        {
            var doc = new XmlDocument();
            doc.LoadXml(xmlRaw);
            var expense = this.parser.Parse(doc);
            Assert.IsTrue(expense.Count == 2);
        }
    }
}