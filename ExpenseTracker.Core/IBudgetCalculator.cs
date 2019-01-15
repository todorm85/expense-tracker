namespace ExpenseTracker.Core
{
    public interface IBudgetCalculator
    {
        decimal CalculateExpectedExpenses(Budget budget);

        decimal CalculateExpectedSavings(Budget budget);

        decimal CalculateExpectedIncome(Budget budget);

        decimal CalculateActualExpenses(Budget budget);

        decimal CalculateActualSavings(Budget budget);
    }
}