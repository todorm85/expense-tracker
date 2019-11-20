using System;

namespace ExpenseTracker.UI
{
    public class MenuActionAttribute : Attribute
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