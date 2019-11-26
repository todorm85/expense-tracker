using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class BudgetService : BaseDataItemService<Budget>, IBudgetService
    {
        public BudgetService(IUnitOfWork uow) : base(uow)
        {
        }

        public Budget GetCumulativeForMonth(DateTime month)
        {
            var allForMonth = this.GetAll(x => x.FromMonth <= month && month <= x.ToMonth);
            if (allForMonth.Count() > 0)
            {
                var baseBudget = new Budget() { ExpectedTransactions = new List<Transaction>() };
                return allForMonth.Aggregate(baseBudget, (x, y) =>
                {
                    x.ExpectedTransactions.AddRange(y.ExpectedTransactions);
                    return new Budget()
                    {
                        ExpectedTransactions = x.ExpectedTransactions,
                        FromMonth = new DateTime(month.Year, month.Month, 1),
                        ToMonth = new DateTime(month.Year, month.Month, 1).AddMonths(1).AddDays(-1)
                    };
                });
            }
            else
            {
                return null;
            }
        }
    }
}