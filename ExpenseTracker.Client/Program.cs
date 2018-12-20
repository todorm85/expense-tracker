using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using ExpenseTracker.Core;
using ExpenseTracker.Data;

namespace ExpenseTracker.ConsoleClient
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            new MainMenu().Run();
        }
    }
}