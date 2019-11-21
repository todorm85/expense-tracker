using System;
using System.IO;
using System.Linq;
using ExpenseTracker.AllianzTxtParser;
using ExpenseTracker.Core;
using ExpenseTracker.Core.UI;
using ExpenseTracker.UI;

namespace ExpenseTracker.App
{
    internal class MainMenu : Menu
    {
        private readonly ITransactionsService transactionsService;

        public MainMenu(
            ITransactionsService transactionsService,
            CategoriesMenu cat,
            BudgetMenu bud,
            ExpensesMenu exp)
        {
            this.Children = new Menu[] { cat, bud, exp };
            this.transactionsService = transactionsService;
            this.AddAction("ba", () => "backup database", () => this.BackupFile(Application.DbPath));

            this.AddAction("im", () => "Import text", () => this.Import());
        }

        public void BackupFile(string sourcePath)
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

        public void Import()
        {
            Console.Write("Provide path to file: ");
            var filePath = Console.ReadLine();

            var parser = new TxtFileParser();
            var ts = parser.ParseFromFile(filePath).Where(t => t.Type == TransactionType.Expense);

            this.transactionsService.Add(ts);
        }
    }
}
