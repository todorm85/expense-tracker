using System;
using System.Collections.Generic;
using ExpenseTracker.Core;
using ExpenseTracker.ExcelConnector;

namespace ExpenseTracker.ExcelExporter
{
    public class ExpensesExcelFile : IExpensesExporter, IExpensesImporter
    {
        public ExpensesExcelFile(string excelFilePath)
        {
            this.filePath = excelFilePath;
        }

        public void Export(IEnumerable<Expense> expenses)
        {
            using (var context = new ExcelContext(this.filePath))
            {
                foreach (var expense in expenses)
                {
                    var row = expense.MapToExcelRow();
                    context.InsertRow(row);
                }

                context.Save();
            }
        }

        public void Export(Dictionary<DateTime, Dictionary<string, decimal>> categoriesByMonth)
        {
            using (var context = new ExcelContext(this.filePath))
            {
                foreach (var month in categoriesByMonth)
                {
                    context.InsertRow(new string[] { month.Key.ToShortDateString() });
                    foreach (var category in month.Value)
                    {
                        context.InsertRow(new string[] { category.Key, category.Value.ToString() });
                    }

                    context.InsertEmptyRow();
                }
            }
        }

        public void Export(Dictionary<DateTime, IEnumerable<Expense>> expensesByMonth)
        {
            using (var context = new ExcelContext(this.filePath))
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
            }
        }

        public IEnumerable<Expense> Import()
        {
            using (var context = new ExcelContext(this.filePath))
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

                return expenses;
            }
        }

        private string filePath;
    }
}