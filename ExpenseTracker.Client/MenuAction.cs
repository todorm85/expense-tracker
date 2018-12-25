using System;

namespace ExpenseTracker.ConsoleClient
{
    public class MenuAction
    {
        public string Command { get; set; }

        public Func<string> GetDescription { get; set; }

        public Action Callback { get; set; }
    }
}