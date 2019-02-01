using System;
using ExpenseTracker.UI;

namespace ExpenseTracker.ConsoleClient
{
    internal class Renderer : IOutputRenderer
    {
        public bool Confirm(string msg = "Confirm? y/n: ")
        {
            var response = this.PromptInput("Save?", "y");
            return response == "y";
        }

        public string PromptInput(string msg, string defaultValue = "")
        {
            Console.WriteLine(msg);
            System.Windows.Forms.SendKeys.SendWait(defaultValue);
            return Console.ReadLine();
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

        public void WriteLine(string value, Style style)
        {
            this.Write(value, style);
            Console.WriteLine();
        }
    }
}