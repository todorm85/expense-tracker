using System;
using System.Collections.Generic;
using System.IO;
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
            if (!File.Exists(excelFilePath))
            {
                this.book = excel.Workbooks.Add();
                this.book.SaveAs(excelFilePath);
            }
            else
            {
                this.book = excel.Workbooks.Open(excelFilePath);
            }

            try
            {
                this.sheet = (Excel.Worksheet)this.book.Sheets[1];
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

        internal void InsertRow(IEnumerable<string> row)
        {
            var lastRow = this.GetFirstFreeRow();
            int col = 1;
            foreach (var rowValue in row)
            {
                this.SetCellValue(lastRow, col++, rowValue);
            }
        }

        public void Save()
        {
            this.book.Save();
        }

        public void Dispose()
        {
            this.book.Close();
        }

        internal void InsertEmptyRow()
        {
            this.InsertRow(new string[] { "" });
        }

        internal List<List<dynamic>> GetRows()
        {
            var lastRow = this.GetFirstFreeRow();
            var rows = new List<List<dynamic>>();
            for (int i = --lastRow; i > 1; i--)
            {
                var row = new List<dynamic>();
                for (int colIndex = 1; colIndex <= ExpenseMappingExtensions.OrderedPropertyNames.Count; colIndex++)
                {
                    var cellVal = this.GetCellValue(i, colIndex);
                    row.Add(cellVal);
                }

                rows.Add(row);
            }

            return rows;
        }

        public int GetFirstFreeRow()
        {
            return this.sheet.Cells.SpecialCells(XlCellType.xlCellTypeLastCell).Row + 1;
        }

        public object GetCellValue(int i, int colIndex)
        {
            return ((Range)this.sheet.Cells[i, colIndex]).Value;
        }

        private Workbook book;
        private Worksheet sheet;
    }
}