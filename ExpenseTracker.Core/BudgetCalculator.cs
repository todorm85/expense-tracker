using System;
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

        public decimal CalculateActualExpenses(DateTime from, DateTime to)
        {
            return this.transactionsService.GetAll()
                                    .Where(e => e.Date >= from
                                        && e.Date <= to
                                        && e.Type == TransactionType.Expense)
                                    .Sum(e => e.Amount);
        }

        public decimal CalculateActualIncome(DateTime from, DateTime to)
        {
            return this.transactionsService.GetAll()
                                    .Where(e => e.Date >= from
                                        && e.Date <= to
                                        && e.Type == TransactionType.Income)
                                    .Sum(e => e.Amount);
        }

        public decimal CalculateActualSavings(DateTime from, DateTime to)
        {
            var actualExpense = this.CalculateActualExpenses(from, to);
            var actualIncome = this.CalculateActualIncome(from, to);
            var actualSaving = actualIncome - actualExpense;
            return actualSaving;
        }

        private readonly ITransactionsService transactionsService;
    }
}