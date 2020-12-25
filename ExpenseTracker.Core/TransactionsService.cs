using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class TransactionsService : BaseDataItemService<Transaction>, ITransactionsService
    {
        public TransactionsService(IUnitOfWork data) : base(data)
        { }

        public void Add(IEnumerable<Transaction> expenses, out IEnumerable<Transaction> added)
        {
            added = Enumerable.Empty<Transaction>();
            foreach (var expense in expenses)
            {
                if (string.IsNullOrWhiteSpace(expense.TransactionId))
                    expense.GenerateTransactionId();
            }

            var filtered = expenses.Where(newTran => !this.IsDuplicate(newTran));
            if (filtered.Count() != 0)
            {
                added = filtered.ToList(); // the query returns a different result after the objects have been added to DB
                base.Add(filtered);
            }
        }

        public override void Add(IEnumerable<Transaction> expenses)
        {
            this.Add(expenses, out IEnumerable<Transaction> ts);
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

        private bool IsDuplicate(Transaction t)
        {
            return this.GetDuplicates(t).Count() != 0;
        }

        public IEnumerable<Transaction> GetDuplicates(Transaction t)
        {
            if (string.IsNullOrWhiteSpace(t.TransactionId))
                t.GenerateTransactionId();
            return this.repo.GetAll(x => x.TransactionId == t.TransactionId);
        }
    }
}