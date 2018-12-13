using System.Collections.Generic;
using ExpenseTracker.Core;
using ExpenseTracker.ExcelConnector;

namespace ExpenseTracker.ExcelExporter
{
    public class ExpensesExcelFile : IExpensesExporter
    {
        public ExpensesExcelFile(string excelFilePath)
        {
            this.filePath = excelFilePath;
        }

        public void Export(IEnumerable<Expense> expenses)
        {
            using (var context = new ExcelContext(this.filePath))
            {
                var lastRow = context.GetFirstFreeRow();
                foreach (var expense in expenses)
                {
                    var row = expense.MapToExcelRow();
                    for (int i = 0; i < row.Count; i++)
                    {
                        context.SetCellValue(lastRow, i + 1, row[i]);
                    }

                    lastRow++;
                }

                context.Save();
            }
        }

        public List<Expense> Get()
        {
            using (var context = new ExcelContext(this.filePath))
            {
                var lastRow = context.GetFirstFreeRow();
                var expenses = new List<Expense>();
                for (int i = --lastRow; i > 1; i--)
                {
                    var rowValues = new List<dynamic>();
                    var orderedProps = ExpenseMappingExtensions.OrderedPropertyNames;
                    for (int colIndex = 1; colIndex <= orderedProps.Count; colIndex++)
                    {
                        var cellVal = context.GetCellValue(i, colIndex);
                        rowValues.Add(cellVal);
                    }

                    expenses.Add(rowValues.MapRowToExpense());
                }

                return expenses;
            }
        }

        private string filePath;
    }
}