﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;

namespace ExpenseTracker.UI
{
    public class MenuBase
    {
        protected IList<MenuAction> menuActions = new List<MenuAction>();
        private const string ExitCommand = "e";
        private string exitCommandText = "Exit";

        public MenuBase(IOutputRenderer renderer)
        {
            this.Renderer = renderer;
        }

        protected IOutputRenderer Renderer { get; }

        public virtual void Run()
        {
            this.GetActions();
            this.exitCommandText = "Exit " + this.GetType().Name;
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
                    this.Renderer.WriteLine($"{a.Command.PadRight(5)} : {a.GetDescription()}");
                }

                this.Renderer.WriteLine($"{exitCommand.PadRight(5)} : {exitText}");

                response = this.Renderer.PromptInput("");
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