namespace ExpenseTracker.Core
{
    public static class StringExtensions
    {
        public static string RemoveRepeatingSpaces(this string s)
        {
            while (true)
            {
                s = s.Replace("  ", " ");
                if (s.IndexOf("  ") < 0)
                {
                    break;
                }
            }

            return s.Trim();
        }
    }
}
