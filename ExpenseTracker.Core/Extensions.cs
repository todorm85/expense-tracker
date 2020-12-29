using System;

namespace ExpenseTracker.Core
{
    public static class Extensions
    {
        public static DateTime ToMonthEnd(this DateTime date)
        {
            return date.ToMonthStart().AddMonths(1).AddMinutes(-1);
        }

        public static DateTime ToMonthStart(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

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