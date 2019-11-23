using System.Configuration;
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