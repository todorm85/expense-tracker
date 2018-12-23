using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Core
{
    public class BudgetService
    {
        private IUnitOfWork uow;

        public BudgetService(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        public IEnumerable<Budget> GetAll()
        {
            return this.uow.Budgets.GetAll();
        }

        public void Add(Budget budget)
        {
            if (this.GetByMonth(budget.Month) != null)
            {
                throw new ArgumentException($"Budget for that month {budget.Month} already exists.");
            }

            this.uow.Budgets.Insert(new Budget[] { budget });
        }

        public void Delete(DateTime month)
        {
            var budgetToRemove = GetByMonth(month);
            if (budgetToRemove == null)
            {
                throw new ArgumentException("Budget for that month does not exists.");
            }

            this.uow.Budgets.Remove(budgetToRemove);
        }

        public Budget GetByMonth(DateTime month)
        {
            return this.GetAll().FirstOrDefault(x => x.Month == month);
        }

        public void Update(Budget budget)
        {
            this.uow.Budgets.Update(new Budget[] { budget });
        }
    }
}
