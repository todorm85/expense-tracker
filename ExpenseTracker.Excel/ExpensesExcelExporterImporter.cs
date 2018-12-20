using System;
using System.Collections.Generic;
using ExpenseTracker.Core;
using ExpenseTracker.ExcelConnector;

namespace ExpenseTracker.ExcelExporter
{
    public class ExpensesExcelExporterImporter
    {
        private ExpensesService service;

        public ExpensesExcelExporterImporter(ExpensesService service)
        {
            this.service = service;
        }
        
        public void ExportCategoriesByMonth(string filePath, DateTime fromDate, DateTime toDate)
        {
            var categoriesByMonth = this.service.GetCategoriesCostByMonth(fromDate, toDate);
            using (var context = new ExcelContext(filePath))
            {
                foreach (var month in categoriesByMonth)
                {
                    context.InsertRow(new string[] { month.Key.ToString("MMMM yyyy") });
                    foreach (var category in month.Value)
                    {
                        context.InsertRow(new string[] { "", category.Key, category.Value.ToString() });
                    }

                    context.InsertEmptyRow();
                }

                context.Save();
            }
        }

        public void ExportExpensesByMonth(string filePath, DateTime fromDate, DateTime toDate)
        {
            var expensesByMonth = this.service.GetExpensesByMonths(fromDate, toDate);
            using (var context = new ExcelContext(filePath))
            {
                foreach (var month in expensesByMonth)
                {
                    context.InsertRow(new string[] { month.Key.ToShortDateString() });
                    foreach (var expense in month.Value)
                    {
                        context.InsertRow(expense.MapToExcelRow());
                    }

                    context.InsertEmptyRow();
                }

                context.Save();
            }
        }

        public void Import(string filePath)
        {
            using (var context = new ExcelContext(filePath))
            {
                var expenses = new List<Expense>();
                var rows = context.GetRows();
                foreach (var row in rows)
                {
                    var expense = row.MapRowToExpense();
                    if (string.IsNullOrEmpty(expense.TransactionId))
                    {
                        expense.TransactionId = Guid.NewGuid().ToString();
                    }

                    expenses.Add(expense);
                }

                this.service.Add(expenses);
            }
        }
    }
}