using System;
using System.Collections.Generic;
using System.Configuration;
using ExpenseTracker.Core;
using ExpenseTracker.Data;
using ExpenseTracker.ExcelExporter;
using ExpenseTracker.GmailConnector;

namespace ExpenseTracker.Client
{
    internal class Program
    {
        private static void Main(string[] args)
        {
            var user = ConfigurationManager.AppSettings["user"];
            var pass = ConfigurationManager.AppSettings["pass"];
            var xlFilePath = ConfigurationManager.AppSettings["excelFilePath"];

            var mailClient = new ExpensesMailClient(user, pass);
            var repo = new ExpensesRepo();
            
            //var excelFile = new ExcelFile(xlFilePath);

            var classifier = new ExpensesClassifier(new Dictionary<string, string>());
            var importer = new ExpensesService(mailClient, repo, classifier);

            importer.Import();
        }
    }
}