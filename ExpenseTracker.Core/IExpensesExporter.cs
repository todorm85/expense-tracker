using System;
using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public interface IExpensesExporter
    {
        void Export(IEnumerable<Expense> expenses);
        void Export(Dictionary<DateTime, Dictionary<string, decimal>> categoriesByMonth);
        void Export(Dictionary<DateTime, IEnumerable<Expense>> expensesByMonth);
    }
}