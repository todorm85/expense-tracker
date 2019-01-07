using System.Linq;

namespace ExpenseTracker.Core
{
    public static class BudgetExtensions
    {
        public static decimal GetExpectedExpenses(this Budget budget)
        {
            return budget.ExpectedExpensesByCategory.Sum(x => x.Value);
        }

        public static decimal GetExpectedSavings(this Budget budget)
        {
            return budget.ExpectedIncome - budget.GetExpectedExpenses();
        }
    }
}