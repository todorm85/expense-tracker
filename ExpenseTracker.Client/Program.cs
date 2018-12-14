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
            var xlFilePath = ConfigurationManager.AppSettings["excelFilePath"];
            var mailClient = GetMailClient();
            var repo = new ExpensesRepo();
            var service = new ExpensesService(mailClient, repo);

            service.Import();
        }

        private static IExpensesMessagesClient GetMailClient()
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
    }
}