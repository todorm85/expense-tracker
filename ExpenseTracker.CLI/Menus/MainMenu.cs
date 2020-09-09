using System;
using System.Collections.Generic;
using System.IO;
using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.Core;
using ExpenseTracker.Core.UI;
using ExpenseTracker.UI;

namespace ExpenseTracker.App
{
    internal class MainMenu : Menu
    {
        private readonly ITransactionsService transactionsService;
        private readonly IBaseDataItemService<Category> categories;
        private readonly MailImporter mailImporter;
        private readonly AllianzTxtFileParser allianzFileParser;
        private readonly RaiffeizenTxtFileParser raiFileParser;
        private readonly Config config;

        public MainMenu(
            ITransactionsService transactionsService,
            IBaseDataItemService<Category> categories,
            MailImporter gmailClient,
            AllianzTxtFileParser fileParser,
            RaiffeizenTxtFileParser raiFileParser,
            CategoriesMenu cat,
            BudgetMenu bud,
            ExpensesMenu exp,
            Config config)
        {
            this.transactionsService = transactionsService;
            this.categories = categories;
            this.mailImporter = gmailClient;
            this.allianzFileParser = fileParser;
            this.raiFileParser = raiFileParser;
            this.config = config;
            this.CommandDescription = "Main menu";

            AddExpensesMenu(exp);
            AddCategoriesMenu(cat);
            AddMisceallaneous();

            this.AddChild(cat);
            this.AddChild(bud);
            this.AddChild(exp);
        }

        private void AddMisceallaneous()
        {
            var misc = new Menu();
            misc.AddAction("clear", () => "Clear all transactions", () => this.Clear());
            misc.AddAction("ba", () => "backup database", () => this.BackupFile(this.config.DbPath));
            misc.CommandKey = "misc";
            misc.CommandDescription = "Misc commands";
            this.AddChild(misc);
        }

        private void AddExpensesMenu(ExpensesMenu exp)
        {
            this.AddAction("sc", () => "show expenses by categories", () => exp.ShowExpensesCategoriesOnly(), "Expenses queries");
            this.AddAction("s", () => "show expenses by categories with details", () => exp.ShowExpensesAll(), "Expenses queries");

            this.AddAction("ae", () => "add", () => exp.QuickAddExpense(), "Expenses edit");
            this.AddAction("de", () => "del", () => exp.Remove(), "Expenses edit");
            this.AddAction("ee", () => "edit", () => exp.Edit(), "Expenses edit");
            this.AddAction("cl", () => "classify all", () => ClassifyAllTransactions(), "Expenses edit");

            this.AddAction("edf", () => "set expenses date filters", () => exp.SetDateFilters(), "Expenses filters");
            this.AddAction("ecf", () => "set expenses category filter", () => exp.SetCategoryFilters(), "Expenses filters");

            this.AddAction("im", () => "Import expenses from mails", () => this.ImportExpensesEmail(), "Connectors");
            this.AddAction("ime", () => "Import expenses from files", () => this.ImportExpensesFiles(), "Connectors");
        }

        private void ClassifyAllTransactions()
        {
            var all = this.transactionsService.GetAll();
            new TransactionsClassifier().Classify(all,this.categories.GetAll());
            this.transactionsService.Update(all);
        }

        private void AddCategoriesMenu(CategoriesMenu cat)
        {
            this.AddAction("scg", () => "show categories by groups", () => cat.ShowAllCategoryGroups(), "Categories");
            this.AddAction("rec", () => "remove category", () => cat.Remove(), "Categories");
            this.AddAction("adc", () => "add category", () => cat.QuickAdd(), "Categories");
            this.AddAction("ec", () => "edit category", () => cat.Edit(), "Categories");
        }

        private void ImportExpensesEmail()
        {
            this.mailImporter.ImportTransactions();
        }

        private void Clear()
        {
            var respons = this.PromptInput("Are you sure?", "n");
            if (respons != "y")
            {
                return;
            }

            transactionsService.Remove(transactionsService.GetAll());
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

        public void ImportExpensesFiles()
        {
            Console.Write("Provide path to files: ");
            var dirPath = Console.ReadLine().Trim('"');
            IEnumerable<Transaction> expenses = this.allianzFileParser.GetTransactions(dirPath + "\\stmtBG24BUIN95611000591258.txt");
            this.transactionsService.Add(expenses);
            expenses = this.allianzFileParser.GetTransactions(dirPath + "\\stmtBG94BUIN95611000529567.txt");
            this.transactionsService.Add(expenses);
            expenses = raiFileParser.ParseFile(dirPath + "\\report.xml");
            this.transactionsService.Add(expenses);
            Console.Write("Done!");
        }
    }
}
