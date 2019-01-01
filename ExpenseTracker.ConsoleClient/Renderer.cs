using System;
using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.UI;

namespace ExpenseTracker.ConsoleClient
{
    internal class Renderer : IOutputRenderer
    {
        public string PromptInput(string msg, string defaultValue = "")
        {
            Console.WriteLine(msg);
            System.Windows.Forms.SendKeys.SendWait(defaultValue);
            return Console.ReadLine();
        }

        public void PromptMenuActions(IEnumerable<MenuAction> actions, string exitCommand, string exitText)
        {
            string response = null;
            while (response != exitCommand)
            {
                foreach (var a in actions)
                {
                    Console.WriteLine($"{a.Command.PadRight(5)} : {a.GetDescription()}");
                }

                Console.WriteLine($"{exitCommand.PadRight(5)} : {exitText}");

                response = Console.ReadLine();
                var action = actions.FirstOrDefault(a => a.Command == response);
                if (action != null)
                {
                    action.Callback();
                }
            }
        }

        public void Write(string value, Style style)
        {
            switch (style)
            {
                case Style.MoreInfo:
                    Console.ForegroundColor = ConsoleColor.DarkGray;
                    break;
                case Style.Error:
                    Console.ForegroundColor = ConsoleColor.Red;
                    break;
                case Style.Success:
                    Console.ForegroundColor = ConsoleColor.Green;
                    break;
                case Style.Primary:
                default:
                    break;
            }

            this.Write(value);
            Console.ResetColor();
        }

        public void Write(string value)
        {
            Console.Write(value);
        }

        public void WriteLine()
        {
            Console.WriteLine();
        }

        public void WriteLine(string value)
        {
            Console.WriteLine(value);
        }
    }
}