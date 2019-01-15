using System.Linq;

namespace ExpenseTracker.Core
{
    public class BudgetCalculator : IBudgetCalculator
    {
        public BudgetCalculator(ISalaryService service, IExpensesService expensesService)
        {
            this.salarySvc = service;
            this.expensesService = expensesService;
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
            return budget.ExpectedIncome + this.salarySvc.SalaryAmount;
        }

        public decimal CalculateActualExpenses(Budget budget)
        {
            return this.expensesService.GetAll()
                                    .Where(e => e.Date.Year == budget.Month.Year && e.Date.Month == budget.Month.Month).Sum(e => e.Amount);
        }

        public decimal CalculateActualSavings(Budget budget)
        {
            var actualExpense = this.CalculateActualExpenses(budget);
            var actualSaving = budget.ActualIncome - actualExpense;
            return actualSaving;
        }

        private readonly ISalaryService salarySvc;
        private readonly IExpensesService expensesService;
    }
}