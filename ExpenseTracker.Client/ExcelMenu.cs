using System;
using System.Configuration;
using ExpenseTracker.Core;
using ExpenseTracker.ExcelExporter;

namespace ExpenseTracker.ConsoleClient
{
    internal class ExcelMenu
    {
        public ExcelMenu(ExpensesService svc)
        {
            this.excelFile = new ExpensesExcelExporterImporter(svc);
        }

        public void Run()
        {
            string response = null;
            while (response != "e")
            {
                Console.WriteLine(@"
ie: import expenses
ee: export expenses (by month)
ec: export categories (by month)
e: end");

                response = Console.ReadLine();
                switch (response)
                {
                    case "ie":
                        ImportExcel();
                        break;
                    case "ee":
                        ExportExcel();
                        break;
                    case "ec":
                        ExportCategoriesExcel();
                        break;
                    default:
                        break;
                }
            }
        }

        private void ExportCategoriesExcel()
        {
            var path = this.GetExcelOutputPath();
            this.excelFile.ExportCategoriesByMonth(path, DateTime.MinValue, DateTime.MaxValue);
        }

        private void ExportExcel()
        {
            var path = this.GetExcelOutputPath();
            this.excelFile.ExportExpensesByMonth(path, DateTime.MinValue, DateTime.MaxValue);
        }

        private void ImportExcel()
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