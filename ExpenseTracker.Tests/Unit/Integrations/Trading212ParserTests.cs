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
            Assert.AreEqual(2, transactions.Count);

            var transaction1 = transactions[0];
            Assert.AreEqual("b8383e4d-36c5-4bea-8bb4-0c9e8e07f933", transaction1.TransactionId);
            Assert.AreEqual(new DateTime(2024, 10, 31, 6, 3, 23, 274, DateTimeKind.Utc), transaction1.Date); // Adjusted to UTC
            Assert.AreEqual("Bank Transfer - Merchant1 - Category1", transaction1.Details);
            Assert.AreEqual(200.00m, transaction1.Amount);
            Assert.AreEqual(TransactionType.Income, transaction1.Type);

            var transaction2 = transactions[1];
            Assert.AreEqual("580945c8-93db-4637-9284-fbfa9d7ae2f3", transaction2.TransactionId);
            Assert.AreEqual(new DateTime(2024, 11, 1, 6, 32, 55, 32, DateTimeKind.Utc), transaction2.Date); // Adjusted to UTC
            Assert.AreEqual("Online Shopping - Merchant2 - Category2", transaction2.Details);
            Assert.AreEqual(150.75m, transaction2.Amount);
            Assert.AreEqual(TransactionType.Expense, transaction2.Type);
        }

        [TestMethod]
        public void Parse_InvalidHeader_ShouldThrowException()
        {
            // Arrange
            var csvData = "WrongHeader,Time,Notes,ID,Total,Currency (Total),Merchant name,Merchant category,ATM Withdrawal Fee\n" +
                          "Card debit,2024-10-31 08:03:23.274,Bank Transfer,b8383e4d-36c5-4bea-8bb4-0c9e8e07f933,200.00,BGN,Merchant1,Category1,";

            // Act & Assert
            var ex = Assert.ThrowsException<ArgumentException>(() => _parser.Parse(csvData));
            Assert.IsTrue(ex.Message.Contains("Unexpected header format"));
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
    }
}
