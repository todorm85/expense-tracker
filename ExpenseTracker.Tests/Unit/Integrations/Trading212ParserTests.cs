using System;
using System.Collections.Generic;
using ExpenseTracker.Integrations.Files;
using ExpenseTracker.Core.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class Trading212ParserTests
    {
        private Trading212CsvParser _parser;

        [TestInitialize]
        public void Setup()
        {
            _parser = new Trading212CsvParser();
        }

        [TestMethod]
        public void Parse_ValidData_ShouldParseTransactionsCorrectly()
        {
            // Arrange
            var csvData = "Action,Time,Notes,ID,Total,Currency (Total),Merchant name,Merchant category,ATM Withdrawal Fee\n" +
                          "Card debit,2024-10-31 08:03:23.274,Bank Transfer,b8383e4d-36c5-4bea-8bb4-0c9e8e07f933,200.00,BGN,Merchant1,Category1,\n" +
                          "Card debit,2024-11-01 08:32:55.032,Online Shopping,580945c8-93db-4637-9284-fbfa9d7ae2f3,-150.75,BGN,Merchant2,Category2,\n" +
                          "Deposit,2024-10-31 08:03:23.274,Bonus,ee650cb9-7553-4880-b961-f49952f1b29a,50.00,BGN,,,";

            // Act
            var transactions = _parser.Parse(csvData);

            // Assert
            Assert.AreEqual(3, transactions.Count);

            var transaction1 = transactions[0];
            Assert.AreEqual("b8383e4d-36c5-4bea-8bb4-0c9e8e07f933", transaction1.TransactionId);
            Assert.AreEqual(new DateTime(2024, 10, 31, 6, 3, 23, 274, DateTimeKind.Utc), transaction1.Date); // Adjusted to UTC
            Assert.AreEqual("Card debit. Bank Transfer - Merchant1 - Category1", transaction1.Details);
            Assert.AreEqual(200.00m, transaction1.Amount);
            Assert.AreEqual(TransactionType.Income, transaction1.Type);

            var transaction2 = transactions[1];
            Assert.AreEqual("580945c8-93db-4637-9284-fbfa9d7ae2f3", transaction2.TransactionId);
            Assert.AreEqual(new DateTime(2024, 11, 1, 6, 32, 55, 32, DateTimeKind.Utc), transaction2.Date); // Adjusted to UTC
            Assert.AreEqual("Card debit. Online Shopping - Merchant2 - Category2", transaction2.Details);
            Assert.AreEqual(150.75m, transaction2.Amount);
            Assert.AreEqual(TransactionType.Expense, transaction2.Type);

            var transaction3 = transactions[2];
            Assert.AreEqual("ee650cb9-7553-4880-b961-f49952f1b29a", transaction3.TransactionId);
            Assert.AreEqual(new DateTime(2024, 10, 31, 6, 3, 23, 274, DateTimeKind.Utc), transaction3.Date); // Adjusted to UTC
            Assert.AreEqual("Deposit. Bonus", transaction3.Details);
            Assert.AreEqual(50.00m, transaction3.Amount);
            Assert.AreEqual(TransactionType.Income, transaction3.Type);
        }

        [TestMethod]
        public void Parse_InvalidHeader_ShouldThrowException()
        {
            // Arrange

            // Missing the "Action" field which is required
            var csvData = "WrongHeader,Notes,Time,ID,Total,Currency (Total),Merchant name,Merchant category,ATM Withdrawal Fee\n" +
                          "Card debit,2024-10-31 08:03:23.274,Bank Transfer,b8383e4d-36c5-4bea-8bb4-0c9e8e07f933,200.00,BGN,Merchant1,Category1,";

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => _parser.Parse(csvData));
            const string ExpectedErrorMessage = "Missing required field: Action";
            Assert.IsTrue(ex.Message.Contains(ExpectedErrorMessage), $"Expected `{ex.Message}` to equal `{ExpectedErrorMessage}`");
        }

        [TestMethod]
        public void Parse_InvalidDateFormat_ShouldThrowException()
        {
            // Arrange
            var csvData = "Action,Time,Notes,ID,Total,Currency (Total),Merchant name,Merchant category,ATM Withdrawal Fee\n" +
                          "Card debit,INVALID_DATE,Bank Transfer,b8383e4d-36c5-4bea-8bb4-0c9e8e07f933,200.00,BGN,Merchant1,Category1,";

            // Act & Assert
            var ex = Assert.ThrowsException<Exception>(() => _parser.Parse(csvData));
            Assert.IsTrue(ex.Message.Contains("Invalid date format."));
        }

        [TestMethod]
        public void Parse_EmptyFile_ShouldReturnEmptyList()
        {
            // Arrange
            var csvData = "Action,Time,Notes,ID,Total,Currency (Total),Merchant name,Merchant category,ATM Withdrawal Fee\n";

            // Act
            var transactions = _parser.Parse(csvData);

            // Assert
            Assert.AreEqual(0, transactions.Count);
        }

        [TestMethod]
        public void Parse_ValidDataWithCurrencyConversionFee_ShouldParseTransactionsCorrectly()
        {
            // Arrange
            var csvData = "Action,Time,Notes,ID,Total,Currency (Total),Currency conversion fee,Currency (Currency conversion fee),Merchant name,Merchant category\n" +
                          "Card debit,2025-01-01 15:25:30,,61ddddc3-f734-4cbc-a133-be29ace6347f,-125.16,BGN,,,FANTASTICO GROUP LTD,RETAIL_STORES\n" +
                          "Card debit,2025-01-02 08:38:20,,cdfc25aa-d804-46c6-a5bc-55aa16fd6d8b,-44.45,BGN,,,BILLA 127 01,RETAIL_STORES\n" +
                          "Card debit,2025-01-02 09:35:14,,4ec32ce5-9a62-4b77-8c1e-e6dff9787d1a,-57.96,BGN,,,TECHNOPOLIS BULGARIA E,MISCELLANEOUS\n" +
                          "Card debit,2025-01-02 09:40:46,,56bb893c-cc57-46ab-9d81-d406a22d7f3c,-11.58,BGN,,,DOUBLE DELIGHT OOD,RESTAURANTS\n" +
                          "Card debit,2025-01-02 10:18:31,,5eeed4e7-9a02-4737-b231-1417ca6b4b91,-4.99,BGN,,,BILLA 127 01,RETAIL_STORES\n" +
                          "Card debit,2025-01-02 11:58:42,,ca7eedb9-fc4b-4e48-8b67-a97f52455656,-50.00,BGN,,,034 MOL BULGARIA,MISCELLANEOUS\n" +
                          "Card debit,2025-01-02 11:59:34,,684abc68-a1f3-4086-bf7e-cae800fe710b,-50.00,BGN,,,034 MOL BULGARIA,MISCELLANEOUS\n";

            // Act
            var transactions = _parser.Parse(csvData);

            // Assert
            Assert.AreEqual(7, transactions.Count);

            var transaction1 = transactions[0];
            Assert.AreEqual("61ddddc3-f734-4cbc-a133-be29ace6347f", transaction1.TransactionId);
            Assert.AreEqual(new DateTime(2025, 1, 1, 13, 25, 30, DateTimeKind.Utc), transaction1.Date); // Adjusted to UTC
            Assert.AreEqual("Card debit. FANTASTICO GROUP LTD - RETAIL_STORES", transaction1.Details);
            Assert.AreEqual(125.16m, transaction1.Amount);
            Assert.AreEqual(TransactionType.Expense, transaction1.Type);

            var transaction2 = transactions[1];
            Assert.AreEqual("cdfc25aa-d804-46c6-a5bc-55aa16fd6d8b", transaction2.TransactionId);
            Assert.AreEqual(new DateTime(2025, 1, 2, 6, 38, 20, DateTimeKind.Utc), transaction2.Date); // Adjusted to UTC
            Assert.AreEqual("Card debit. BILLA 127 01 - RETAIL_STORES", transaction2.Details);
            Assert.AreEqual(44.45m, transaction2.Amount);
            Assert.AreEqual(TransactionType.Expense, transaction2.Type);

            // Add assertions for the remaining transactions similarly
        }

        [TestMethod]
        public void Parse_ValidDataWithDifferentCurrency_ShouldParseTransactionsCorrectly()
        {
            // Arrange
            var csvData = "Action,Time,Notes,ID,Total,Currency (Total),Merchant name,Merchant category,ATM Withdrawal Fee\n" +
                          "Card debit,2025-01-01 15:25:30,,61ddddc3-f734-4cbc-a133-be29ace6347f,-125.16,USD,FANTASTICO GROUP LTD,RETAIL_STORES,\n" +
                          "Card debit,2025-01-02 08:38:20,,cdfc25aa-d804-46c6-a5bc-55aa16fd6d8b,-44.45,EUR,BILLA 127 01,RETAIL_STORES,\n";

            // Act
            var transactions = _parser.Parse(csvData);

            // Assert
            Assert.AreEqual(2, transactions.Count);

            var transaction1 = transactions[0];
            Assert.AreEqual("61ddddc3-f734-4cbc-a133-be29ace6347f", transaction1.TransactionId);
            Assert.AreEqual(new DateTime(2025, 1, 1, 13, 25, 30, DateTimeKind.Utc), transaction1.Date); // Adjusted to UTC
            Assert.AreEqual("!!!!CURRENCY: USD. Card debit. FANTASTICO GROUP LTD - RETAIL_STORES", transaction1.Details);
            Assert.AreEqual(125.16m, transaction1.Amount);
            Assert.AreEqual(TransactionType.Expense, transaction1.Type);

            var transaction2 = transactions[1];
            Assert.AreEqual("cdfc25aa-d804-46c6-a5bc-55aa16fd6d8b", transaction2.TransactionId);
            Assert.AreEqual(new DateTime(2025, 1, 2, 6, 38, 20, DateTimeKind.Utc), transaction2.Date); // Adjusted to UTC
            Assert.AreEqual("!!!!CURRENCY: EUR. Card debit. BILLA 127 01 - RETAIL_STORES", transaction2.Details);
            Assert.AreEqual(44.45m, transaction2.Amount);
            Assert.AreEqual(TransactionType.Expense, transaction2.Type);
        }
    }
}
