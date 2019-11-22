using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExpenseTracker.UI
{
    public class Menu
    {
        protected IList<MenuAction> menuActions = new List<MenuAction>();
        private const string ExitCommand = "e";
        private string exitCommandText = "Exit";
        private string menuCommandName;
        private string menuCommandDescription;

        public Menu()
        {
            this.exitCommandText = "Exit " + this.GetType().Name;

            this.Output = Runtime.Output;
            this.Input = Runtime.Input;

            this.ResolveActionMethods();
            this.menuActions = this.menuActions.Reverse().ToList();
        }

        public virtual string CommandKey
        {
            get
            {
                return this.menuCommandName ?? this.GetType().Name.Substring(0, 3).ToLower();
            }
            set
            {
                this.menuCommandName = value;
            }
        }

        public virtual string CommandDescription
        {
            get
            {
                return this.menuCommandDescription ?? this.GetType().Name;
            }
            set
            {
                this.menuCommandDescription = value;
            }
        }

        public IOutputProvider Output { get; private set; }

        public IInputProvider Input { get; private set; }

        public void AddAction(string command, Func<string> decsription, Action action, string group = "", int ordinal = 0)
        {
            if (command == ExitCommand || this.menuActions.Any(a => a.Command == command))
            {
                throw new ArgumentException("Action with duplicate command found.");
            }

            this.menuActions.Add(new MenuAction()
            {
                Callback = action,
                Command = command,
                GetDescription = decsription,
                Group = group,
                Ordinal = ordinal
            });
        }

        public virtual void Run(bool showActions = false)
        {
            string response = null;
            while (response != ExitCommand)
            {
                if (showActions)
                {
                    this.PrintActions();
                }

                response = this.PromptInput("");
                if (response == "?")
                {
                    this.PrintActions();
                    continue;
                }

                var action = this.menuActions.FirstOrDefault(a => a.Command == response);
                if (action != null)
                {
                    action.Callback();
                }
            }
        }

        public void AddChild(Menu menu)
        {
            this.AddAction(menu.CommandKey, () => menu.CommandDescription, () => menu.Run(), "Submenus");
        }

        private void PrintActions()
        {
            this.Output.WriteLine($"{this.CommandDescription}");
            this.WriteDelimiter();
            var groups = this.menuActions.GroupBy(x => x.Group).OrderBy(x => x.Key);
            foreach (var group in groups)
            {
                if (!string.IsNullOrWhiteSpace(group.Key))
                {
                    this.Output.WriteLine(group.Key);
                    this.WriteDelimiter();
                }

                foreach (var a in group.OrderBy(x => x.Ordinal))
                {
                    this.Output.WriteLine($"{a.Command.PadRight(5)} : {a.GetDescription()}");
                }

                this.WriteDelimiter();
            }

            this.Output.WriteLine($"{ExitCommand.PadRight(5)} : {this.exitCommandText}");
        }

        private void WriteDelimiter()
        {
            this.Output.WriteLine($"{new string('-', 35)}");
        }

        protected virtual void ResolveActionMethods()
        {
            var methods = this.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Where(mi => mi.GetCustomAttributes().Any(at => at.GetType() == typeof(MenuActionAttribute)));
            foreach (var m in methods)
            {
                var attribute = m.GetCustomAttribute(typeof(MenuActionAttribute)) as MenuActionAttribute;
                this.AddAction(attribute.Command, () => attribute.Description, () => m.Invoke(this, null), attribute.Group);
            }
        }
    }
}