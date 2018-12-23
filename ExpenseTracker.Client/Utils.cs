using System;
using System.IO;

namespace ExpenseTracker.ConsoleClient
{
    public class Utils
    {
        public static string Prompt(string msg, string defaultValue = "")
        {
            Console.WriteLine(msg);
            System.Windows.Forms.SendKeys.SendWait(defaultValue);
            return Console.ReadLine();
        }
    }
}