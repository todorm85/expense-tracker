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

        public Menu()
        {
            this.exitCommandText = "Exit " + this.GetType().Name;
            this.Children = new Menu[0];
        }

        public virtual string MenuCommandName
        {
            get
            {
                return this.GetType().Name.Substring(0, 3).ToLower();
            }
        }

        public virtual string MenuCommandDescription
        {
            get
            {
                return this.GetType().Name;
            }
        }

        public IEnumerable<Menu> Children { get; set; }

        public IOutputProvider Output { get; private set; }

        public IInputProvider Input { get; private set; }

        public virtual void Run(IOutputProvider output, IInputProvider input)
        {
            this.Output = output;
            this.Input = input;
            this.ResolveChildren();
            this.GetActions();
            this.PromptMenuActions(this.menuActions, ExitCommand, this.exitCommandText);
        }

        public void AddAction(string command, Func<string> decsription, Action action)
        {
            if (command == ExitCommand || this.menuActions.Any(a => a.Command == command))
            {
                throw new ArgumentException("Action with duplicate command found.");
            }

            this.menuActions.Add(new MenuAction()
            {
                Callback = action,
                Command = command,
                GetDescription = decsription
            });
        }

        private void PromptMenuActions(IEnumerable<MenuAction> actions, string exitCommand, string exitText)
        {
            string response = null;
            while (response != exitCommand)
            {
                foreach (var a in actions)
                {
                    this.Output.WriteLine($"{a.Command.PadRight(5)} : {a.GetDescription()}");
                }

                this.Output.WriteLine($"{exitCommand.PadRight(5)} : {exitText}");

                response = this.PromptInput("");
                var action = actions.FirstOrDefault(a => a.Command == response);
                if (action != null)
                {
                    action.Callback();
                }
            }
        }

        private void GetActions()
        {
            var methods = this.GetType()
                .GetMethods(BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance)
                .Where(mi => mi.GetCustomAttributes().Any(at => at.GetType() == typeof(MenuActionAttribute)));
            foreach (var m in methods)
            {
                var attribute = m.GetCustomAttribute(typeof(MenuActionAttribute)) as MenuActionAttribute;
                this.AddAction(attribute.Command, () => attribute.Description, () => m.Invoke(this, null));
            }
        }

        private void ResolveChildren()
        {
            foreach (var childMenu in this.Children)
            {
                this.AddAction(childMenu.MenuCommandName, () => childMenu.MenuCommandDescription, () => childMenu.Run(this.Output, this.Input));
            }
        }
    }
}