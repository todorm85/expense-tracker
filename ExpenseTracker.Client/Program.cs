using System;
using System.Configuration;
using System.IO;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using ExpenseTracker.ExcelExporter;
using ExpenseTracker.GmailConnector;

namespace ExpenseTracker.Client
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            string response = null;
            while (response != "e")
            {
                Console.WriteLine(@"
e: end,
im: Imports From GMail
ie: imports from excel
ee: exports to excel
c: categorizes expenses
");

                response = Console.ReadLine();
                switch (response)
                {
                    case "im":
                        ImportGmail();
                        break;

                    case "ie":
                        ImportExcel();
                        break;
                    case "ee":
                        ExportExcel();
                        break;

                    default:
                        break;
                }
            }
        }

        private static void ExportExcel()
        {
            var path = GetExcelInputPath();
            var excelFile = new ExpensesExcelFile(path);
            var service = new ExpensesService(new IExpensesImporter[] { }, new IExpensesExporter[] { excelFile }, GetRepo());
            service.ExportByMonths(DateTime.MinValue, DateTime.MaxValue);
        }

        private static void ImportExcel()
        {
            var xlInput = GetExcelInputPath();
            var excelFile = new ExpensesExcelFile(xlInput);
            var service = new ExpensesService(new IExpensesImporter[] { excelFile }, new IExpensesExporter[] { }, GetRepo());
            service.Import();
        }

        private static string GetExcelInputPath()
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

        private static string GetExcelOutputPath()
        {
            var xlOutput = ConfigurationManager.AppSettings["xlOutput"];
            xlOutput = Environment.ExpandEnvironmentVariables(xlOutput);
            if (string.IsNullOrEmpty(xlOutput))
            {
                throw new ArgumentNullException("xl output path is null");
            }

            return xlOutput;
        }

        private static void ImportGmail()
        {
            var mailClient = GetMailClient();
            var service = new ExpensesService(new IExpensesImporter[] { mailClient }, new IExpensesExporter[0], GetRepo());

            service.Import();
        }

        private static IExpensesImporter GetMailClient()
        {
            string user, pass;
            GetCredentials(out user, out pass);
            var mailClient = new ExpensesMailClient(user, pass);
            return mailClient;
        }

        private static void GetCredentials(out string user, out string pass)
        {
            user = ConfigurationManager.AppSettings["user"];
            if (string.IsNullOrEmpty(user))
            {
                Console.WriteLine("Mail:");
                user = Console.ReadLine();
            }

            pass = ConfigurationManager.AppSettings["pass"];
            if (string.IsNullOrEmpty(pass))
            {
                Console.WriteLine("Pass:");
                pass = Console.ReadLine();
            }
        }

        private static IExpensesRepository GetRepo()
        {
            var path = ConfigurationManager.AppSettings["dbPath"];
            path = Environment.ExpandEnvironmentVariables(path);
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("Enter valid db path:");
            }

            return new ExpensesRepo(path);
        }
    }
}