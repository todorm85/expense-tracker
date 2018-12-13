using System.Collections.Generic;

namespace ExpenseTracker.Core
{
    public interface IExpensesExporter
    {
        void Export(IEnumerable<Expense> expenses);
    }
}