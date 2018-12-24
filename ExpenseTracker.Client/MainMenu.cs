using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using ExpenseTracker.Core;
using ExpenseTracker.ExcelExporter;
using ExpenseTracker.GmailConnector;

namespace ExpenseTracker.ConsoleClient
{
    internal class MainMenu : MenuBase
    {
        public MainMenu()
        {
            this.service = ServicesFactory.GetService<ExpensesService>();
            this.AddAction("xl", "Excel menu", () => new ExcelMenu().Run());
            this.AddAction("c", "Categories menu", () => new CategoriesMenu().Run());
            this.AddAction("ex", "Expenses menu", () => new ExpensesMenu().Run());
            this.AddAction("bu", "Budget menu", () => new BudgetMenu().Run());
        }

        [MenuAction("b", "Backup database")]
        public void BackupFile()
        {
            string sourcePath = Utils.GetDbPath();
            var rootPath = Path.GetDirectoryName(sourcePath);
            var baseFileName = Path.GetFileNameWithoutExtension(sourcePath);
            var extension = Path.GetExtension(sourcePath);

            int i = 0;
            var fileName = baseFileName + "_" + i;
            var newPath = rootPath + "\\" + fileName + "." + extension;
            while (File.Exists(newPath))
            {
                i++;
                fileName = baseFileName + "_" + i;
                newPath = rootPath + "\\" + fileName + "." + extension;
            }

            File.Copy(sourcePath, newPath);
        }

        [MenuAction("im", "Import GMail")]
        public void ImportGmail()
        {
            this.GetMailClient().Import();
        }

        [MenuAction("cl", "Classify all expenses")]
        public void Categorize()
        {
            this.service.Classify();
        }

        private IDictionary<string, string> GetKeysCategories()
        {
            var path = ConfigurationManager.AppSettings["classifierSettingsPath"];
            path = Environment.ExpandEnvironmentVariables(path);
            if (path != null)
            {
                return new CategoriesJsonParser().ParseFile(path);
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

        private ExpensesService service;
        private ExpensesExcelExporterImporter excelFile;
    }
}