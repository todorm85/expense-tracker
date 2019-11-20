using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class TransactionsService : BaseDataItemService<Transaction>, ITransactionsService
    {
        public TransactionsService(IUnitOfWork data) : base(data)
        {
            this.uow = data;
        }

        private TransactionsClassifier Classifier
        {
            get
            {
                if (this._classifier == null)
                {
                    this._classifier = new TransactionsClassifier(this.uow.GetDataItemsRepo<Category>().GetAll());
                }

                return this._classifier;
            }
        }

        public override void Add(IEnumerable<Transaction> expenses)
        {
            expenses = expenses.Where(newTran => string.IsNullOrEmpty(newTran.TransactionId) // new items
                || this.repo.GetAll(t => t.TransactionId == newTran.TransactionId && t.Source == newTran.Source) == null); // there are transactions with duplicate ids -> withdrawal and taxes for the withdrawal are two transactions with same transaction reference for example

            if (expenses.Count() != 0)
            {
                this.Classifier.Classify(expenses);
                base.Add(expenses);
            }
        }

        public void Classify()
        {
            var msgs = this.repo.GetAll().ToList();
            this.Classifier.Classify(msgs);
            this.repo.Update(msgs);
        }

        public Dictionary<DateTime, Dictionary<string, IEnumerable<Transaction>>> GetExpensesByCategoriesByMonths(DateTime fromDate, DateTime toDate)
        {
            var expenses = this.repo.GetAll(x => x.Date >= fromDate && x.Date <= toDate && x.Type == TransactionType.Expense);
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

        private readonly IUnitOfWork uow;
        private TransactionsClassifier _classifier;
    }
}