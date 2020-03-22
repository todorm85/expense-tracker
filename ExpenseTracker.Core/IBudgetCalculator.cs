using System;

namespace ExpenseTracker.Core
{
    public interface IBudgetCalculator
    {
        decimal CalculateExpectedExpenses(Budget budget);

        decimal CalculateExpectedSavings(Budget budget);

        decimal CalculateExpectedIncome(Budget budget);

        decimal CalculateActualIncome(DateTime from, DateTime to);

        decimal CalculateActualExpenses(DateTime from, DateTime to);

        decimal CalculateActualSavings(DateTime from, DateTime to);
    }
}