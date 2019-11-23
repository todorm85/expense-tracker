using System;
using System.Collections.Generic;
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
            this.transactionsService = transactionsService;
            this.CommandDescription = "Main menu";

            AddExpensesMenu(exp);
            AddCategoriesMenu(cat);
            AddAllianzMenu();
            AddMisceallaneous();

            this.AddChild(cat);
            this.AddChild(bud);
            this.AddChild(exp);
        }

        private void AddMisceallaneous()
        {
            var misc = new Menu();
            misc.AddAction("clear", () => "Clear all transactions", () => this.Clear());
            misc.AddAction("ba", () => "backup database", () => this.BackupFile(Application.DbPath));
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
            this.AddAction("cl", () => "classify all", () => exp.Categorize(), "Expenses edit");

            this.AddAction("edf", () => "set expenses date filters", () => exp.SetDateFilters(), "Expenses filters");
            this.AddAction("ecf", () => "set expenses category filter", () => exp.SetCategoryFilters(), "Expenses filters");

            this.AddAction("im", () => "Import expenses from Allianz mails", () => this.ImportExpensesGmail(), "Connectors");
        }

        private void AddCategoriesMenu(CategoriesMenu cat)
        {
            this.AddAction("scg", () => "show categories by groups", () => cat.ShowAllCategoryGroups(), "Categories");
            this.AddAction("rec", () => "remove category", () => cat.Remove(), "Categories");
            this.AddAction("adc", () => "add category", () => cat.QuickAdd(), "Categories");
            this.AddAction("ec", () => "edit category", () => cat.Edit(), "Categories");
        }

        private void AddAllianzMenu()
        {
            var allianz = new Menu();
            allianz.AddAction("ime", () => "Import expenses from Allianz text", () => this.ImportExpensesText(), "Allianz Group");
            allianz.AddAction("ims", () => "Import income from Allianz text", () => this.ImportSalary(), "Allianz Group");
            allianz.CommandKey = "al";
            allianz.CommandDescription = "allianz";
            this.AddChild(allianz);
        }

        private void ImportExpensesGmail()
        {
            this.transactionsService.Add(
                new GmailConnector.GmailClient().Get(
                    System.Text.Encoding.ASCII.GetString(Convert.FromBase64String(Environment.GetEnvironmentVariable("trckrm", EnvironmentVariableTarget.User))),
                    Environment.GetEnvironmentVariable("trckr", EnvironmentVariableTarget.User)));
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

        public void ImportExpensesText()
        {
            string filePath = PromptFilePath();
            var parser = new TxtFileParser();
            IEnumerable<Transaction> expenses = parser.GetTransactions(TransactionType.Expense, filePath);
            this.transactionsService.Add(expenses);
        }

        private void ImportSalary()
        {
            string filePath = PromptFilePath();
            var parser = new TxtFileParser();
            this.transactionsService.Add(parser.GetSalary(filePath));
        }

        private static string PromptFilePath()
        {
            Console.Write("Provide path to file: ");
            var filePath = Console.ReadLine();
            return filePath;
        }
    }
}
