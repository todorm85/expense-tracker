﻿using System;
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

            this.CommandDescription = "Expenses main menu";

            this.AddAction("se", () => "show expenses by categories", () => exp.ShowExpensesCategoriesOnly(), "Expenses");
            this.AddAction("sed", () => "show expenses by categories with details", () => exp.ShowExpensesAll(), "Expenses");
            this.AddAction("ae", () => "add", () => exp.QuickAddExpense(), "Expenses");
            this.AddAction("de", () => "del", () => exp.Remove(), "Expenses");
            this.AddAction("cl", () => "classify all", () => exp.Categorize(), "Expenses");

            this.AddAction("scg", () => "show categories by groups", () => cat.ShowAll(), "Categories");
            this.AddAction("rec", () => "remove category", () => cat.Remove(), "Categories");
            this.AddAction("adc", () => "add category", () => cat.Add(), "Categories");

            var allianz = new Menu();
            allianz.AddAction("ime", () => "Import expenses from Allianz text", () => this.ImportExpenses(), "Allianz Group");
            allianz.AddAction("ims", () => "Import income from Allianz text", () => this.ImportSalary(), "Allianz Group");
            allianz.CommandKey = "al";
            allianz.CommandDescription = "allianz";
            this.AddChild(allianz);

            var misc = new Menu();
            misc.AddAction("clear", () => "Clear all transactions", () => this.Clear());
            misc.AddAction("ba", () => "backup database", () => this.BackupFile(Application.DbPath));
            misc.CommandKey = "misc";
            misc.CommandDescription = "Misc commands";
            this.AddChild(misc);

            this.AddChild(cat);
            this.AddChild(bud);
            this.AddChild(exp);
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

        public void ImportExpenses()
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
