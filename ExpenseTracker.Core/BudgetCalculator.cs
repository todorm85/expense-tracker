using System;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class BudgetCalculator : IBudgetCalculator
    {
        private readonly ITransactionsService transactionsService;

        public BudgetCalculator(ITransactionsService expensesService)
        {
            this.transactionsService = expensesService;
        }

        public decimal CalculateActualExpenses(DateTime from, DateTime to)
        {
            return this.transactionsService.GetAll(e => e.Date >= from
                                        && e.Date <= to
                                        && e.Type == TransactionType.Expense
                                        && !e.Ignored)
                                    .Sum(e => e.Amount);
        }

        public decimal CalculateActualIncome(DateTime from, DateTime to)
        {
            return this.transactionsService.GetAll(e => e.Date >= from
                                        && e.Date <= to
                                        && e.Type == TransactionType.Income
                                        && !e.Ignored)
                                    .Sum(e => e.Amount);
        }

        public decimal CalculateActualSavings(DateTime from, DateTime to)
        {
            var actualExpense = this.CalculateActualExpenses(from, to);
            var actualIncome = this.CalculateActualIncome(from, to);
            var actualSaving = actualIncome - actualExpense;
            return actualSaving;
        }

        public decimal CalculateExpectedExpenses(Budget budget)
        {
            return budget.ExpectedTransactions
                .Where(x => x.Type == TransactionType.Expense)
                .Sum(x => x.Amount);
        }

        public decimal CalculateExpectedIncome(Budget budget)
        {
            return budget.ExpectedTransactions
                .Where(x => x.Type == TransactionType.Income)
                .Sum(x => x.Amount); ;
        }

        public decimal CalculateExpectedSavings(Budget budget)
        {
            return this.CalculateExpectedIncome(budget) - this.CalculateExpectedExpenses(budget);
        }
    }
}