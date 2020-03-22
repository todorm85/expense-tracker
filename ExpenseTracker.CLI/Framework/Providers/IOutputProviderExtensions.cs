using System;

namespace ExpenseTracker.UI
{
    public static class IOutputProviderExtensions
    {
        public static void Write(this IOutputProvider renderer, string value, Style style)
        {
            var oldStyle = renderer.Style;
            renderer.Style = style;
            renderer.Write(value);
            renderer.Style = oldStyle;
        }

        public static void WriteLine(this IOutputProvider renderer, string value, Style style = Style.Primary)
        {
            var oldStyle = renderer.Style;
            renderer.Style = style;
            renderer.Write(value);
            renderer.NewLine();
            renderer.Style = oldStyle;
        }
    }
}