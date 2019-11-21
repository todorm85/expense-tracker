using ExpenseTracker.Core;
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ExpenseTracker.UI
{
    internal static class PropertyExtensions
    {
        public static String GetAdditionalInfo(this PropertyInfo pi)
        {
            if (typeof(List<Transaction>) == pi.PropertyType)
            {
                return "(category:source:type:amount;)";
            }

            return string.Empty;
        }
    }
}
