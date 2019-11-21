using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;
using ExpenseTracker.App;

namespace ExpenseTracker.ConsoleClient
{
    internal class Program
    {
        public static void Main(string[] args)
        {
            var renderer = new IOProvider();
            new Application(ConfigurationManager.AppSettings["dbPath"], renderer, renderer);
        }
    }
}