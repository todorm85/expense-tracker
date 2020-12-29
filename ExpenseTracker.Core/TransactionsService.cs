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

        public override void Add(Transaction item)
        {
            this.Add(new Transaction[] { item });
        }

        public IEnumerable<Transaction> GetDuplicates(Transaction t)
        {
            if (string.IsNullOrWhiteSpace(t.TransactionId))
                t.GenerateTransactionId();
            return this.repo.GetAll(x => x.TransactionId == t.TransactionId);
        }

        public List<List<Transaction>> GetPotentialDuplicates()
        {
            var orderedTransactions = this.repo.GetAll().OrderByDescending(x => x.Date)
                .ThenByDescending(x => x.Amount)
                .ThenByDescending(x => x.Details);
            List<List<Transaction>> result = new List<List<Transaction>>();
            Transaction lastTransaction = null;
            var currentBatch = new List<Transaction>();
            foreach (var t in orderedTransactions)
            {
                if (lastTransaction == null)
                {
                    lastTransaction = t;
                    continue;
                }

                if (lastTransaction.Date.Year == t.Date.Year
                    && lastTransaction.Date.Month == t.Date.Month
                    && lastTransaction.Date.Day == t.Date.Day
                    && lastTransaction.Amount == t.Amount)
                {
                    currentBatch.Add(t);
                }
                else
                {
                    lastTransaction = t;
                    if (currentBatch.Count > 1 && currentBatch.Any(x => !x.IsResolvedDuplicate))
                    {
                        currentBatch.ForEach(x => x.IsResolvedDuplicate = false);
                        this.Update(currentBatch);
                        result.Add(currentBatch);
                    }

                    currentBatch = new List<Transaction>();
                    currentBatch.Add(t);
                }
            }

            return result;
        }

        private bool IsDuplicate(Transaction t)
        {
            return this.GetDuplicates(t).Count() != 0;
        }
    }
}