using System;
using ExpenseTracker.ExcelExporter;
using Microsoft.Office.Interop.Excel;

using Excel = Microsoft.Office.Interop.Excel;

namespace ExpenseTracker.ExcelConnector
{
    internal class ExcelContext : IDisposable
    {
        public ExcelContext(string excelFilePath)
        {
            var excel = new Excel.Application();
            excel.Visible = false;
            try
            {
                this.book = excel.Workbooks.Open(excelFilePath);
                this.sheet = (Excel.Worksheet)this.book.Sheets[1];
                this.ValidateHeaderRow();
            }
            catch (Exception)
            {
                this.book.Close();
                throw;
            }
        }

        public void SetCellValue(int row, int col, string value)
        {
            this.sheet.Cells[row, col] = value;
        }

        public void Save()
        {
            this.book.Save();
        }

        public void Dispose()
        {
            this.book.Close();
        }

        public int GetFirstFreeRow()
        {
            return this.sheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell).Row + 1;
        }

        public object GetCellValue(int i, int colIndex)
        {
            return ((Range)this.sheet.Cells[i, colIndex]).Value;
        }

        private void ValidateHeaderRow()
        {
            var propOrder = ExpenseMappingExtensions.OrderedPropertyNames;
            for (int i = 0; i < propOrder.Count; i++)
            {
                var propName = propOrder[i];
                var cell = (Range)this.sheet.Cells[1, i + 1];
                var cellValue = cell.Value as string;
                if (string.IsNullOrEmpty(cellValue))
                {
                    cell.Value = propName;
                }
                else if (cellValue != propName)
                {
                    throw new InvalidOperationException("Excel sheet column order does not match the importer mapping order.");
                }
            }
        }

        private Workbook book;
        private Worksheet sheet;
    }
}