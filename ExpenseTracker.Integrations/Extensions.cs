using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Integrations
{
    internal static class Extensions
    {
        public static DateTime ConvertToUtcFromBgTimeUnknowKind(this DateTime parsedDate)
        {
            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Unspecified);
            var bgTz = TimeZoneInfo.FindSystemTimeZoneById("FLE Standard Time");
            var offset = bgTz.GetUtcOffset(parsedDate);
            parsedDate = parsedDate.Add(-offset);
            parsedDate = DateTime.SpecifyKind(parsedDate, DateTimeKind.Utc);
            return parsedDate;
        }
    }
}
