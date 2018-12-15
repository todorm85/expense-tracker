using System;
using System.Collections.Generic;
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

        internal void InsertRow(List<string> row)
        {
            var lastRow = this.GetFirstFreeRow();
            for (int i = 0; i < row.Count; i++)
            {
                this.SetCellValue(lastRow, i + 1, row[i]);
            }

            lastRow++;
        }

        public void Save()
        {
            this.book.Save();
        }

        public void Dispose()
        {
            this.book.Close();
        }

        internal List<List<dynamic>> GetRows()
        {
            var lastRow = this.GetFirstFreeRow();
            var rows = new List<List<dynamic>>();
            for (int i = --lastRow; i > 1; i--)
            {
                var row = new List<dynamic>();
                for (int colIndex = 1; colIndex <= orderedProps.Count; colIndex++)
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