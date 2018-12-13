using System;
using System.Collections.Generic;
using ExpenseTracker.Core;
using ExpenseTracker.ExcelExporter;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace ExpenseTracker.Tests
{
    [TestClass]
    public class ExpensesExcelListExporterTests
    {
        private string path = @"tests.xlsx";

        [TestMethod]
        public void Insert_InsertsExpensesCorrectly()
        {
            var exporter = new ExpensesExcelFile(path);
            exporter.Export(TestExpensesFactory.GetTestExpenses(10));
        }

        [TestMethod]
        public void Get_GetsExpensesCorrectly()
        {
            var exporter = new ExpensesExcelFile(path);
            var expenses = exporter.Get();
        }

    }
}
