using System;
using System.IO;
using ExpenseTracker.AllianzTxtParser;
using ExpenseTracker.Core;
using ExpenseTracker.Core.UI;
using ExpenseTracker.GmailConnector;
using ExpenseTracker.UI;
using Unity;

namespace ExpenseTracker.ConsoleClient
{
    public class MenuBuilder
    {
        private readonly IUnityContainer unity;
        private readonly Settings settings;

        public MenuBuilder(IUnityContainer unity)
        {
            this.unity = unity;
            this.settings = unity.Resolve<Settings>();
        }

        public virtual Menu Build()
        {
            var main = this.unity.Resolve<Menu>();
            main.AddAction("c", () => "Categories menu", () => this.unity.Resolve<CategoriesMenu>().Run());
            main.AddAction("ex", () => "Expenses menu", () => this.unity.Resolve<ExpensesMenu>().Run());
            main.AddAction("bu", () => "Budget menu", () => this.unity.Resolve<BudgetMenu>().Run());

            if (this.settings.ClientMode != Settings.WebClientModeValue)
            {
                main.AddAction("ba", () => "backup database", () => this.BackupFile(this.settings.DbPath));
            }

            main.AddAction("im", () => "Import text", () => this.Import());
            return main;
        }

        private void BackupFile(string sourcePath)
        {
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

        private void Import()
        {
            Console.Write("Provide path to file: ");
            var filePath = Console.ReadLine();

            var parser = new TxtFileParser();
            var ts = parser.ParseFromFile(filePath);

            var service = this.unity.Resolve<ITransactionsService>();
            service.Add(ts);
        }
    }
}
