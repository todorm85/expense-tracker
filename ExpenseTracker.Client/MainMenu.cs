using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using ExpenseTracker.ExcelExporter;
using ExpenseTracker.GmailConnector;

namespace ExpenseTracker.ConsoleClient
{
    internal class MainMenu
    {
        public MainMenu()
        {
            this.service = new ExpensesService(this.GetRepo(), this.GetKeysCategories());
        }

        public void Run()
        {
            string response = null;
            while (response != "e")
            {
                Console.WriteLine(@"
im: imports From GMail
ex: excel menu
c: categorizes expenses
q: query menu
e: end");

                response = Console.ReadLine();
                switch (response)
                {
                    case "im":
                        ImportGmail();
                        break;
                    case "ex":
                        new ExcelMenu(this.service).Run();
                        break;                    
                    case "c":
                        Categorize();
                        break;
                    case "q":
                        new QueryMenu(this.service).Run();
                        break;
                    default:
                        break;
                }
            }
        }

        private void Categorize()
        {
            this.service.Classify();
        }

        private void ImportGmail()
        {
            this.GetMailClient().Import();
        }

        private IDictionary<string, string> GetKeysCategories()
        {
            var path = ConfigurationManager.AppSettings["classifierSettingsPath"];
            path = Environment.ExpandEnvironmentVariables(path);
            if (path != null)
            {
                return new CategoriesKeyphrasesJsonParser().ParseFile(path);
            }
            else
            {
                return new Dictionary<string, string>();
            }
        }

        private ExpensesGmailImporter GetMailClient()
        {
            string user;
            string pass;
            this.GetCredentials(out user, out pass);
            return new ExpensesGmailImporter(user, pass, this.service);
        }

        private void GetCredentials(out string user, out string pass)
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

        private IExpensesRepository GetRepo()
        {
            var path = ConfigurationManager.AppSettings["dbPath"];
            path = Environment.ExpandEnvironmentVariables(path);
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("Enter valid db path:");
            }

            return new ExpensesRepo(path);
        }

        private ExpensesService service;
        private ExpensesExcelExporterImporter excelFile;
    }
}