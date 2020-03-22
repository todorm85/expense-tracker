using System;
using System.Globalization;

namespace ExpenseTracker.UI
{
    public static class MenuExtensions
    {
        public static void PromptDateFilter(this Menu menu, ref DateTime fromDate, ref DateTime toDate)
        {
            const string Format = "M/yy";
            var result = menu.PromptInput("Filter (Date from month Date to month)", $"{fromDate.ToString(Format, CultureInfo.InvariantCulture)} {toDate.ToString(Format, CultureInfo.InvariantCulture)}");
            fromDate = DateTime.ParseExact(result.Split(' ')[0], Format, CultureInfo.InvariantCulture);
            toDate = DateTime.ParseExact(result.Split(' ')[1], Format, CultureInfo.InvariantCulture);
        }

        public static string PromptInput(this Menu menu, string msg, string def = "")
        {
            menu.Output.Write(msg);
            return menu.Input.Read(def);
        }

        public static void ShowActualExpected(this Menu menu, decimal primary, decimal secondary, bool secondaryShouldBeHigher = true, bool renderDiff = true)
        {
            menu.Output.Write($"{primary}");
            menu.Output.Write($" {secondary}", Style.MoreInfo);
            if (renderDiff)
            {
                menu.Output.Write(" ");
                menu.ShowDiff(secondary - primary, secondaryShouldBeHigher);
            }
        }

        public static void ShowActualExpectedNewLine(this Menu menu, decimal primary, decimal secondary, bool secondaryShouldBeHigher = true, bool renderDiff = true)
        {
            menu.ShowActualExpected(primary, secondary, secondaryShouldBeHigher, renderDiff);
            menu.Output.Write($" (actual");
            menu.Output.Write($" | ");
            menu.Output.Write($"expected", Style.MoreInfo);
            menu.Output.Write($" | ");
            menu.Output.Write($"saved", Style.Success);
            menu.Output.WriteLine(")");
        }

        public static void ShowDiff(this Menu menu, decimal amount, bool positiveIsGood = true)
        {
            var style = amount >= 0 && positiveIsGood ? Style.Success : Style.Error;
            menu.Output.Write(amount.ToString(), style);
        }

        public static bool Confirm(this Menu menu, string msg = "Confirm y/n")
        {
            menu.Output.Write(msg);
            string choice = string.Empty;
            while (choice != "y" && choice != "n")
            {
                choice = menu.Input.Read();
            }

            return choice.ToLower() == "y";
        }
    }
}
