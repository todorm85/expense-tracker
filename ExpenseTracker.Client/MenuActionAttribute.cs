using System;

namespace ExpenseTracker.ConsoleClient
{
    internal class MenuActionAttribute : Attribute
    {
        public MenuActionAttribute(string command, string description)
        {
            this.Command = command;
            this.Description = description;
        }

        public string Command { get; private set; }

        public string Description { get; private set; }
    }
}