namespace ExpenseTracker.UI
{
    internal static class IOutputRendererExtensions
    {
        public static void RenderDiffernce(this IOutputRenderer renderer, decimal primary, decimal secondary, string prefix = "", bool secondaryShouldBeHigher = true)
        {
            renderer.Write($"{prefix}{primary}");
            renderer.Write($" {secondary}", Style.MoreInfo);
            var diff = secondaryShouldBeHigher ? secondary - primary : primary - secondary;
            var style = diff >= 0 ? Style.Success : Style.Error;
            renderer.Write($" {diff}", style);
        }

        public static void RenderDiffernceNewLine(this IOutputRenderer renderer, decimal primary, decimal secondary, string prefix = "", bool secondaryShouldBeHigher = true, bool noNewLine = false)
        {
            renderer.RenderDiffernce(primary, secondary, prefix, secondaryShouldBeHigher);
            renderer.WriteLine();
        }
    }
}