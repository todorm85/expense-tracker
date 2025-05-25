using ExpenseTracker.Core.Services;
using ExpenseTracker.Core.Transactions;
using System;
using System.Linq;

namespace ExpenseTracker.Core.Budget
{
    public class BudgetCalculator : IBudgetCalculator
    {
        private readonly IExpensesService transactionsService;

        public BudgetCalculator(IExpensesService expensesService)
        {
            this.transactionsService = expensesService;
        }

        public decimal CalculateActualExpenses(DateTime from, DateTime to)
        {
            return this.transactionsService.GetAll(e => e.Date >= from
                                        && e.Date <= to
                                        && e.Type == TransactionType.Expense
                                        && (e.Category == null || !e.Category.Contains(Constants.IgnoredCategory, StringComparison.OrdinalIgnoreCase)))
                                    .Sum(e => e.Amount);
        }

        public decimal CalculateActualIncome(DateTime from, DateTime to)
        {
            return this.transactionsService.GetAll(e => e.Date >= from
                                        && e.Date <= to
                                        && e.Type == TransactionType.Income
                                        && (e.Category == null || !e.Category.Contains(Constants.IgnoredCategory, StringComparison.OrdinalIgnoreCase)))
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