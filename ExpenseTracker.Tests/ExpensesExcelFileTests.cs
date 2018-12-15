using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using ExpenseTracker.Core;
using ExpenseTracker.ExcelExporter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class ExpensesExcelFileTests
    {
        private string path = $@"{Environment.GetFolderPath(Environment.SpecialFolder.Desktop)}\tests.xlsx";

        [TestCleanup]
        public void Cleanup()
        {
            if (File.Exists(path))
                File.Delete(path);
        }

        [TestMethod]
        public void ExportsAndImportsExpensesCorrectly()
        {
            var excelFile = new ExpensesExcelFile(path);
            excelFile.Export(TestExpensesFactory.GetTestExpenses(10));
            var expenses = excelFile.Import();
            Assert.AreEqual(10, expenses.Count());
        }
    }
}
