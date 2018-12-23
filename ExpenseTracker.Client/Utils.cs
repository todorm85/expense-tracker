using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using ExpenseTracker.Core;

namespace ExpenseTracker.ConsoleClient
{
    public class Utils
    {
        public static string PromptInput(string msg, string defaultValue = "")
        {
            Console.WriteLine(msg);
            System.Windows.Forms.SendKeys.SendWait(defaultValue);
            return Console.ReadLine();
        }

        public static void PromptMenuActions(IEnumerable<MenuAction> actions, string exitCommand, string exitText)
        {
            string response = null;
            while (response != exitCommand)
            {
                foreach (var a in actions)
                {
                    Console.WriteLine($"{a.Command.PadRight(5)} : {a.Description}");
                }

                Console.WriteLine($"{exitCommand.PadRight(5)} : {exitText}");

                response = Console.ReadLine();
                var action = actions.FirstOrDefault(a => a.Command == response);
                if (action != null)
                {
                    action.Callback();
                }
            }
        }

        public static string GetDbPath()
        {
            var path = ConfigurationManager.AppSettings["dbPath"];
            path = Environment.ExpandEnvironmentVariables(path);
            if (string.IsNullOrEmpty(path))
            {
                throw new ArgumentNullException("Enter valid db path:");
            }

            return path;
        }
    }
}