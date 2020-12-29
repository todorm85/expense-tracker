using ExpenseTracker.UI;
using System;
using WindowsInput;

namespace ExpenseTracker.ConsoleClient
{
    internal class IOProvider : IOutputProvider, IInputProvider
    {
        public Style Style { get; set; }

        public void NewLine()
        {
            Console.WriteLine();
        }

        public string Read()
        {
            return Console.ReadLine();
        }

        public string Read(string preenteredValue)
        {
            if (!string.IsNullOrWhiteSpace(preenteredValue))
            {
                new InputSimulator().Keyboard.TextEntry(preenteredValue);
            }

            return Console.ReadLine();
        }

        public void Write(string value)
        {
            switch (this.Style)
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

            Console.Write(value);
            Console.ResetColor();
        }
    }
}