using System.Collections.Generic;
using System.Reflection;

namespace ExpenseTracker.Core.UI
{
    internal static class PropertyExtensions
    {
        public static string GetAdditionalInfo(this PropertyInfo pi)
        {
            if (typeof(List<Transaction>) == pi.PropertyType)
            {
                return "(category:source:type:amount;)";
            }

            return string.Empty;
        }
    }
}