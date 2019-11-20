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

        public Menu(IOutputProvider output, IInputProvider input)
        {
            this.exitCommandText = "Exit " + this.GetType().Name;
            this.Output = output;
            this.Input = input;
        }

        public IOutputProvider Output { get; }

        public IInputProvider Input { get; }

        public virtual void Run()
        {
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
    }
}