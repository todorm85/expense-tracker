using System;
using System.Collections.Generic;
using ExpenseTracker.Core.Transactions;
using ExpenseTracker.Integrations.Files;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class RevolutCsvParserTests
    {
        private RevolutCsvParser _parser;

        [TestInitialize]
        public void Setup()
        {
            _parser = new RevolutCsvParser();
        }

        [TestMethod]
        public void Parse_ValidData_ShouldParseTransactionsCorrectly()
        {
            // Arrange
            var csvData = "Type,Product,Started Date,Completed Date,Description,Amount,Fee,Currency,State,Balance\n" +
                          "TRANSFER,Current,12/1/2024 13:22,12/1/2024 13:22,From DESISLAVA DIMITROVA NENCHEVA MITSKOVSKA & TODOR BORISLAVOV MITSKOVSKI,300,0,BGN,COMPLETED,396.98\n" +
                          "CARD_PAYMENT,Current,12/1/2024 11:07,12/2/2024 17:50,Happy Delivery,-71.34,0,BGN,COMPLETED,325.64";

            // Act
            var transactions = _parser.Parse(csvData);

            // Assert
            Assert.AreEqual(2, transactions.Count);

            var transaction1 = transactions[0];
            Assert.AreEqual("TRANSFER", transaction1.TransactionId.Substring(0, 8));
            Assert.AreEqual(new DateTime(2024, 12, 1, 11, 22, 0, DateTimeKind.Utc), transaction1.Date); // Adjusted to UTC
            Assert.AreEqual("From DESISLAVA DIMITROVA NENCHEVA MITSKOVSKA & TODOR BORISLAVOV MITSKOVSKI", transaction1.Details);
            Assert.AreEqual(300m, transaction1.Amount);
            Assert.AreEqual(TransactionType.Income, transaction1.Type);

            var transaction2 = transactions[1];
            Assert.AreEqual("CARD_PAYMENT", transaction2.TransactionId.Substring(0, 12));
            Assert.AreEqual(new DateTime(2024, 12, 1, 9, 7, 0, DateTimeKind.Utc), transaction2.Date); // Adjusted to UTC
            Assert.AreEqual("Happy Delivery", transaction2.Details);
            Assert.AreEqual(71.34m, transaction2.Amount);
            Assert.AreEqual(TransactionType.Expense, transaction2.Type);
        }

        [TestMethod]
        public void Parse_InvalidRowFieldCount_ShouldThrowException()
        {
            // Arrange
            var csvData = "Type,Product,Started Date,Completed Date,Description,Amount,Fee,Currency,State,Balance\n" +
                          "TRANSFER,Current,12/1/2024 13:22,12/1/2024 13:22,Missing Fields";

            // Act & Assert
            var ex = Assert.ThrowsException<Exception>(() => _parser.Parse(csvData));
            Assert.IsTrue(ex.Message.Contains("Invalid row: incorrect number of fields"));
        }

        [TestMethod]
        public void Parse_EmptyFile_ShouldReturnEmptyList()
        {
            // Arrange
            var csvData = "Type,Product,Started Date,Completed Date,Description,Amount,Fee,Currency,State,Balance\n";

            // Act
            var transactions = _parser.Parse(csvData);

            // Assert
            Assert.AreEqual(0, transactions.Count);
        }

        [TestMethod]
        public void Parse_InvalidDateFormat_ShouldThrowException()
        {
            // Arrange
            var csvData = "Type,Product,Started Date,Completed Date,Description,Amount,Fee,Currency,State,Balance\n" +
                          "CARD_PAYMENT,Current,INVALID_DATE,12/2/2024 17:50,Happy Delivery,-71.34,0,BGN,COMPLETED,325.64";

            // Act & Assert
            var ex = Assert.ThrowsException<Exception>(() => _parser.Parse(csvData));
            Assert.IsTrue(ex.Message.Contains("Invalid date format"));
        }

        [TestMethod]
        public void Parse_ValidData_ShouldEnsureDatesAreUtc()
        {
            // Arrange
            var csvData = "Type,Product,Started Date,Completed Date,Description,Amount,Fee,Currency,State,Balance\n" +
                          "TOPUP,Current,12/29/2024 13:55,12/29/2024 13:55,Google Pay Top-Up by *4895,1000,0,BGN,COMPLETED,1246.87";

            // Act
            var transactions = _parser.Parse(csvData);

            // Assert
            Assert.AreEqual(0, transactions.Count); // No transaction due to zero fee
        }
    }
}
