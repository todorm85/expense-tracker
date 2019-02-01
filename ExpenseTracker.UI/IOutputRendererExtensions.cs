using System;

namespace ExpenseTracker.UI
{
    internal static class IOutputRendererExtensions
    {
        public static void RenderActualExpected(this IOutputRenderer renderer, decimal primary, decimal secondary, bool secondaryShouldBeHigher = true, bool renderDiff = true)
        {
            renderer.Write($"{primary}");
            renderer.Write($" {secondary}", Style.MoreInfo);
            if (renderDiff)
            {
                renderer.Write(" ");
                renderer.RenderDiff(secondary - primary, secondaryShouldBeHigher);
            }
        }

        public static void RenderActualExpectedNewLine(this IOutputRenderer renderer, decimal primary, decimal secondary, bool secondaryShouldBeHigher = true, bool renderDiff = true)
        {
            renderer.RenderActualExpected(primary, secondary, secondaryShouldBeHigher, renderDiff);
            renderer.WriteLine();
        }

        public static void GetDateFilter(this IOutputRenderer renderer, ref DateTime fromDate, ref DateTime toDate)
        {
            var result = renderer.PromptInput("Filter (Date from, Date to)", $"{fromDate.ToShortDateString()} {toDate.ToShortDateString()}");
            fromDate = DateTime.Parse(result.Split(' ')[0]);
            toDate = DateTime.Parse(result.Split(' ')[1]);
        }

        public static void RenderDiff(this IOutputRenderer renderer, decimal amount, bool positiveIsGood = true)
        {
            var style = amount >= 0 && positiveIsGood ? Style.Success : Style.Error;
            renderer.Write(amount.ToString(), style);
        }
    }
}