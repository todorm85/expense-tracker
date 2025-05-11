using System;
using System.Security.Cryptography;
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

        /// <summary>
        /// Computes a short hash (8 hex chars) of the input string using SHA256.
        /// Suitable for uniqueness checks, not for cryptographic security.
        /// </summary>
        /// <param name="rawData">The input string to hash.</param>
        /// <returns>8-character hexadecimal hash string.</returns>
        internal static string ComputeShortHash(this string rawData)
        {
            using var sha256 = SHA256.Create();
            var hash = sha256.ComputeHash(Encoding.UTF8.GetBytes(rawData));
            // Take first 4 bytes (8 hex chars) for a short hash
            return BitConverter.ToString(hash, 0, 4).Replace("-", "").ToLowerInvariant();
        }
    }
}