﻿using Force.Crc32;
using System;
using System.Text;

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

        public static DateTime ToDayStart(this DateTime date)
        {
            return new DateTime(date.Year, date.Month, date.Day, 0, 0, 0, 0);
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

        internal static string ComputeCRC32Hash(this string rawData)
        {
            return Crc32Algorithm.Compute(Encoding.UTF8.GetBytes(rawData)).ToString();
        }
    }
}