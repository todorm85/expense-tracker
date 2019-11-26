using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using ExpenseTracker.Core;

namespace ExpenseTracker.ExcelExporter
{
    public static class ExpenseMappingExtensions
    {
        public static List<string> OrderedPropertyNames
        {
            get
            {
                return new List<string>()
                {
                    "Date",
                    "Source",
                    "Amount",
                    "TransactionId",
                    "Account",
                    "Category"
                };
            }
        }

        public static List<string> MapToExcelRow(this Transaction expense)
        {
            var row = new List<string>();
            var props = GetExpenseProperties();

            foreach (var propName in OrderedPropertyNames)
            {
                var prop = props.First(p => p.Name == propName);
                row.Add(prop.GetValue(expense)?.ToString());
            }

            return row;
        }

        public static Transaction MapRowToExpense(this List<dynamic> row)
        {
            var expense = new Transaction();

            var props = GetExpenseProperties();
            for (int i = 0; i < OrderedPropertyNames.Count(); i++)
            {
                var prop = props.First(p => p.Name == OrderedPropertyNames[i]);
                if (prop.Name == "Amount")
                {
                    prop.SetValue(expense, (decimal)row[i]);
                }
                else
                {
                    prop.SetValue(expense, row[i]);
                }
            }

            return expense;
        }

        private static PropertyInfo[] GetExpenseProperties()
        {
            return typeof(Transaction).GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly);
        }
    }
}