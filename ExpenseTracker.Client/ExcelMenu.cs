using System;
using System.Configuration;
using System.IO;
using ExpenseTracker.Core;
using ExpenseTracker.ExcelExporter;

namespace ExpenseTracker.ConsoleClient
{
    internal class ExcelMenu : MenuBase
    {
        public ExcelMenu()
        {
            this.excelFile = new ExpensesExcelExporterImporter(ServicesFactory.GetService<ExpensesService>());
        }

        [MenuAction("ec", "Export categories (by month)")]
        public void ExportCategoriesExcel()
        {
            var path = this.GetExcelOutputPath();
            this.excelFile.ExportCategoriesByMonth(path, DateTime.MinValue, DateTime.MaxValue);
        }

        [MenuAction("ee", "Export expenses (by month)")]
        public void ExportExcel()
        {
            var path = this.GetExcelOutputPath();
            this.excelFile.ExportExpensesByMonth(path, DateTime.MinValue, DateTime.MaxValue);
        }

        [MenuAction("i", "Import expenses")]
        public void ImportExcel()
        {
            var xlInput = this.GetExcelInputPath();
            this.excelFile.Import(xlInput);
        }

        private string GetExcelInputPath()
        {
            var xlInput = ConfigurationManager.AppSettings["xlInput"];
            xlInput = Environment.ExpandEnvironmentVariables(xlInput);
            while (!File.Exists(xlInput))
            {
                Console.WriteLine("Excel input file does not exist. Enter path");
                xlInput = Console.ReadLine();
            }

            return xlInput;
        }

        private string GetExcelOutputPath()
        {
            var xlOutput = ConfigurationManager.AppSettings["xlOutput"];
            xlOutput = Environment.ExpandEnvironmentVariables(xlOutput);
            if (string.IsNullOrEmpty(xlOutput))
            {
                throw new ArgumentNullException("xl output path is null");
            }

            return xlOutput;
        }

        private ExpensesExcelExporterImporter excelFile;
    }
}