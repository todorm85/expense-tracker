using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class BudgetService : BaseDataItemService<Budget>
    {
        public BudgetService(IUnitOfWork uow) : base(uow)
        {
        }

        public override void Add(IEnumerable<Budget> budgets)
        {
            foreach (var budget in budgets)
            {
                if (this.GetByMonth(budget.Month) != null)
                {
                    throw new ArgumentException($"Budget for that month {budget.Month} already exists.");
                }

            }

            base.Add(budgets);
        }

        public void Remove(DateTime month)
        {
            var budgetToRemove = this.GetByMonth(month);
            if (budgetToRemove == null)
            {
                throw new ArgumentException("Budget for that month does not exists.");
            }

            base.Remove(new Budget[] { budgetToRemove });
        }

        public Budget GetByMonth(DateTime month)
        {
            return this.GetAll().FirstOrDefault(x => x.Month == month);
        }

        private IGenericRepository<Budget> budgets;
    }
}