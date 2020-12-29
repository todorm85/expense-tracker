using System;

namespace ExpenseTracker.Core
{
    public interface IBudgetCalculator
    {
        decimal CalculateActualExpenses(DateTime from, DateTime to);

        decimal CalculateActualIncome(DateTime from, DateTime to);

        decimal CalculateActualSavings(DateTime from, DateTime to);

        decimal CalculateExpectedExpenses(Budget budget);

        decimal CalculateExpectedIncome(Budget budget);

        decimal CalculateExpectedSavings(Budget budget);
    }
}