using System;

namespace ExpenseTracker.Core
{
    public static class Extensions
    {
        public static DateTime SetToBeginningOfMonth(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, 1);
        }

        public static DateTime SetToEndOfMonth(this DateTime date)
        {
            return date.SetToBeginningOfMonth().AddMonths(1).AddMinutes(-1);
        }
    }
}
