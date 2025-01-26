using System;
using System.Collections.Generic;
using ExpenseTracker.Core.Transactions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using ExpenseTracker.Integrations.Files.Base;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class BaseCsvParserTests
    {
        private MockCsvParser _parser;

        [TestInitialize]
        public void Setup()
        {
            _parser = new MockCsvParser(); // Using a mock implementation of BaseCsvParser for testing
        }

        [TestMethod]
        public void ParseFields_ShouldHandleEmptyFields()
        {
            // Arrange
            var line = "Field1,Field2,,Field4,";

            // Act
            var fields = _parser.TestParseFields(line);

            // Assert
            Assert.AreEqual(5, fields.Length);
            Assert.AreEqual("Field1", fields[0]);
            Assert.AreEqual("Field2", fields[1]);
            Assert.AreEqual("", fields[2]); // Empty field
            Assert.AreEqual("Field4", fields[3]);
            Assert.AreEqual("", fields[4]); // Trailing empty field
        }

        [TestMethod]
        public void ParseFields_ShouldHandleNoTrailingFields()
        {
            // Arrange
            var line = "Field1,Field2,Field3,Field4";

            // Act
            var fields = _parser.TestParseFields(line);

            // Assert
            Assert.AreEqual(4, fields.Length);
            Assert.AreEqual("Field1", fields[0]);
            Assert.AreEqual("Field2", fields[1]);
            Assert.AreEqual("Field3", fields[2]);
            Assert.AreEqual("Field4", fields[3]);
        }

        [TestMethod]
        public void ParseFields_ShouldHandleQuotedFields()
        {
            // Arrange
            var line = "Field1,\"Field2, with, commas\",Field3";

            // Act
            var fields = _parser.TestParseFields(line);

            // Assert
            Assert.AreEqual(3, fields.Length);
            Assert.AreEqual("Field1", fields[0]);
            Assert.AreEqual("Field2, with, commas", fields[1]); // Quoted field with commas
            Assert.AreEqual("Field3", fields[2]);
        }

        [TestMethod]
        public void ParseFields_ShouldHandleEmptyLines()
        {
            // Arrange
            var line = "";

            // Act
            var fields = _parser.TestParseFields(line);

            // Assert
            Assert.AreEqual(1, fields.Length);
            Assert.AreEqual("", fields[0]); // Single empty field
        }

        [TestMethod]
        public void ParseFields_ShouldHandleConsecutiveDelimiters()
        {
            // Arrange
            var line = "Field1,,,Field4,";

            // Act
            var fields = _parser.TestParseFields(line);

            // Assert
            Assert.AreEqual(5, fields.Length);
            Assert.AreEqual("Field1", fields[0]);
            Assert.AreEqual("", fields[1]); // Empty field
            Assert.AreEqual("", fields[2]); // Empty field
            Assert.AreEqual("Field4", fields[3]);
            Assert.AreEqual("", fields[4]); // Trailing empty field
        }

        [TestMethod]
        public void ParseFields_ShouldHandleFieldsWithWhitespace()
        {
            // Arrange
            var line = "Field1, ,Field3,  ,Field5";

            // Act
            var fields = _parser.TestParseFields(line);

            // Assert
            Assert.AreEqual(5, fields.Length);
            Assert.AreEqual("Field1", fields[0]);
            Assert.AreEqual(" ", fields[1]); // Field with whitespace
            Assert.AreEqual("Field3", fields[2]);
            Assert.AreEqual("  ", fields[3]); // Field with multiple spaces
            Assert.AreEqual("Field5", fields[4]);
        }

        [TestMethod]
        public void ParseFields_ShouldHandleFieldsWithEmbeddedQuotes()
        {
            // Arrange
            var line = "Field1,\"Field2 \"\"with embedded quotes\"\"\",Field3";

            // Act
            var fields = _parser.TestParseFields(line);

            // Assert
            Assert.AreEqual(3, fields.Length);
            Assert.AreEqual("Field1", fields[0]);
            Assert.AreEqual("Field2 \"with embedded quotes\"", fields[1]); // Field with embedded quotes
            Assert.AreEqual("Field3", fields[2]);
        }

        [TestMethod]
        public void ParseFields_ShouldHandleFieldsWithEmbeddedNewlines()
        {
            // Arrange
            var line = "Field1,\"Field2 with\nembedded newline\",Field3";

            // Act
            var fields = _parser.TestParseFields(line);

            // Assert
            Assert.AreEqual(3, fields.Length);
            Assert.AreEqual("Field1", fields[0]);
            Assert.AreEqual("Field2 with\nembedded newline", fields[1]); // Field with embedded newlines
            Assert.AreEqual("Field3", fields[2]);
        }

        [TestMethod]
        public void ParseFields_ShouldHandleFieldsWithEmbeddedCommas()
        {
            // Arrange
            var line = "Field1,\"Field2, with, multiple, commas\",Field3";

            // Act
            var fields = _parser.TestParseFields(line);

            // Assert
            Assert.AreEqual(3, fields.Length);
            Assert.AreEqual("Field1", fields[0]);
            Assert.AreEqual("Field2, with, multiple, commas", fields[1]); // Field with embedded commas
            Assert.AreEqual("Field3", fields[2]);
        }

        private class MockCsvParser : CsvParserBase<Transaction>
        {
            public string[] TestParseFields(string line)
            {
                return base.ParseFields(line);
            }

            protected override Transaction MapRowToEntity(string[] fields)
            {
                // No-op for testing
                return null;
            }
        }
    }
}
