using System;

namespace ExpenseTracker.UI
{
    public class MenuActionAttribute : Attribute
    {
        public MenuActionAttribute(string command, string description, string group = "")
        {
            this.Command = command;
            this.Description = description;
            this.Group = group;
        }

        public string Command { get; private set; }

        public string Description { get; private set; }

        public string Group { get; private set; }
    }
}