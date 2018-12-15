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
            throw new NotImplementedException();
        }

        public void Export(Dictionary<DateTime, IEnumerable<Expense>> expensesByMonth)
        {
            throw new NotImplementedException();
        }

        public IEnumerable<Expense> Import()
        {
            using (var context = new ExcelContext(this.filePath))
            {
                var expenses = new List<Expense>();
                var rows = context.GetRows();
                foreach (var row in rows)
                {
                    expenses.Add(row.MapRowToExpense());
                }

                return expenses;
            }
        }

        private string filePath;
    }
}