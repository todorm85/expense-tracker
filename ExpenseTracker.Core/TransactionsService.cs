using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class TransactionsService : BaseDataItemService<Transaction>, ITransactionsService
    {
        private readonly IUnitOfWork uow;

        public TransactionsService(IUnitOfWork data) : base(data)
        {
            this.uow = data;
        }

        public override void Add(IEnumerable<Transaction> expenses)
        {
            var filtered = expenses.Where(newTran => !this.IsDuplicate(newTran));
            if (filtered.Count() != 0)
            {
                base.Add(filtered);
            }
        }

        public Dictionary<DateTime, Dictionary<string, IEnumerable<Transaction>>> GetExpensesByCategoriesByMonths(DateTime fromDate, DateTime toDate)
        {
            var expenses = this.repo.GetAll(x => x.Date >= fromDate && x.Date <= toDate && x.Type == TransactionType.Expense && !x.Ignored);
            var byCategoryByMonths = new Dictionary<DateTime, Dictionary<string, IEnumerable<Transaction>>>();
            foreach (var year in expenses.GroupBy(x => x.Date.Year))
            {
                foreach (var month in year.GroupBy(x => x.Date.Month))
                {
                    var categories = new Dictionary<string, IEnumerable<Transaction>>();
                    foreach (var cat in month.GroupBy(x => x.Category))
                    {
                        categories.Add(cat.Key ?? "", cat.ToList());
                    }

                    byCategoryByMonths.Add(new DateTime(year.Key, month.Key, 1), categories);
                }
            }

            return byCategoryByMonths;
        }

        public void Update(Transaction expense)
        {
            this.Update(new Transaction[] { expense });
        }

        private bool IsDuplicate(Transaction t)
        {
            return this.GetDuplicates(t).Count() != 0;
        }

        public IEnumerable<Transaction> GetDuplicates(Transaction t)
        {
            return this.repo.GetAll(x => x.TransactionId == t.TransactionId);
        }
    }
}