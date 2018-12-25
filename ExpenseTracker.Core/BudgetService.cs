using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class BudgetService
    {
        public BudgetService(IUnitOfWork uow)
        {
            this.budgets = uow.GetDataItemsRepo<Budget>();
        }

        public IEnumerable<Budget> GetAll()
        {
            return this.budgets.GetAll();
        }

        public void Add(Budget budget)
        {
            if (this.GetByMonth(budget.Month) != null)
            {
                throw new ArgumentException($"Budget for that month {budget.Month} already exists.");
            }

            this.budgets.Insert(new Budget[] { budget });
        }

        public void Delete(DateTime month)
        {
            var budgetToRemove = this.GetByMonth(month);
            if (budgetToRemove == null)
            {
                throw new ArgumentException("Budget for that month does not exists.");
            }

            this.budgets.Remove(budgetToRemove);
        }

        public Budget GetByMonth(DateTime month)
        {
            return this.GetAll().FirstOrDefault(x => x.Month == month);
        }

        public void Update(Budget budget)
        {
            this.budgets.Update(new Budget[] { budget });
        }

        private IGenericRepository<Budget> budgets;
    }
}