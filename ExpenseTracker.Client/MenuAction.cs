using System;

namespace ExpenseTracker.ConsoleClient
{
    public class MenuAction
    {
        public string Command { get; set; }

        public string Description { get; set; }

        public Action Callback { get; set; }
    }
}