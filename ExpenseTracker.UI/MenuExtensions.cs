using System;

namespace ExpenseTracker.UI
{
    public static class MenuExtensions
    {
        public static void GetDateFilter(this Menu menu, ref DateTime fromDate, ref DateTime toDate)
        {
            var result = menu.PromptInput("Filter (Date from, Date to)", $"{fromDate.ToShortDateString()} {toDate.ToShortDateString()}");
            fromDate = DateTime.Parse(result.Split(' ')[0]);
            toDate = DateTime.Parse(result.Split(' ')[1]);
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
            menu.Output.NewLine();
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
