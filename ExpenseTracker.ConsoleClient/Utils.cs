using ExpenseTracker.GmailConnector;
using ExpenseTracker.UI;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;

namespace ExpenseTracker.ConsoleClient
{
    public class Utils
    {
        public static string GetDbPath()
        {
            var path = ConfigurationManager.AppSettings["dbPath"];
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("Enter valid db path:");
            }

            path = Environment.ExpandEnvironmentVariables(path);
            return path;
        }
    }
}