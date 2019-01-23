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
            return budget.ExpectedTransactions
                .Where(x => x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);
        }

        public decimal CalculateExpectedSavings(Budget budget)
        {
            return this.CalculateExpectedIncome(budget) - this.CalculateExpectedExpenses(budget);
        }

        public decimal CalculateExpectedIncome(Budget budget)
        {
            return budget.ExpectedTransactions
                .Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount); ;
        }

        public decimal CalculateActualExpenses(Budget budget)
        {
            return this.transactionsService.GetAll()
                                    .Where(e => e.Date >= budget.FromMonth
                                        && e.Date <= budget.ToMonth
                                        && e.Type == TransactionType.Expense)
                                    .Sum(e => e.Amount);
        }

        public decimal CalculateActualIncome(Budget budget)
        {
            return this.transactionsService.GetAll()
                                    .Where(e => e.Date >= budget.FromMonth
                                        && e.Date <= budget.ToMonth
                                        && e.Type == TransactionType.Income)
                                    .Sum(e => e.Amount);
        }

        public decimal CalculateActualSavings(Budget budget)
        {
            var actualExpense = this.CalculateActualExpenses(budget);
            var actualIncome = this.CalculateActualIncome(budget);
            var actualSaving = actualIncome - actualExpense;
            return actualSaving;
        }

        private readonly ITransactionsService transactionsService;
    }
}