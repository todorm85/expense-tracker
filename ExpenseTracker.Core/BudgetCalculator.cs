using System.Linq;

namespace ExpenseTracker.Core
{
    public class BudgetCalculator : IBudgetCalculator
    {
        public BudgetCalculator(ITransactionsService expensesService)
        {
            this.transactionsService = expensesService;
        }

        public decimal CalculateExpectedExpenses(Budget budget)
        {
            return budget.ExpectedExpensesByCategory.Sum(x => x.Value);
        }

        public decimal CalculateExpectedSavings(Budget budget)
        {
            return this.CalculateExpectedIncome(budget) - this.CalculateExpectedExpenses(budget);
        }

        public decimal CalculateExpectedIncome(Budget budget)
        {
            return budget.ExpectedIncome;
        }

        public decimal CalculateActualExpenses(Budget budget)
        {
            return this.transactionsService.GetAll()
                                    .Where(e => e.Date.Year == budget.Month.Year 
                                        && e.Date.Month == budget.Month.Month
                                        && e.Type == TransactionType.Expense)
                                    .Sum(e => e.Amount);
        }

        public decimal CalculateActualSavings(Budget budget)
        {
            var actualExpense = this.CalculateActualExpenses(budget);
            var actualSaving = budget.ActualIncome - actualExpense;
            return actualSaving;
        }

        private readonly ITransactionsService transactionsService;
    }
}