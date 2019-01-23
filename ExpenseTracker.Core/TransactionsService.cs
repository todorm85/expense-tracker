using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class TransactionsService : BaseDataItemService<Transaction>, ITransactionsService
    {
        public TransactionsService(IUnitOfWork data) : base(data)
        {
            this.data = data;
        }

        private TransactionsClassifier Classifier
        {
            get
            {
                if (this._classifier == null)
                {
                    this._classifier = new TransactionsClassifier(this.data.GetDataItemsRepo<Category>().GetAll());
                }

                return this._classifier;
            }
        }

        public override void Add(IEnumerable<Transaction> expenses)
        {

            var allExistingTransactionIds = this.repo.GetAll().Select(x => x.TransactionId);
            expenses = expenses.Where(x => string.IsNullOrEmpty(x.TransactionId) || !allExistingTransactionIds.Contains(x.TransactionId));

            this.Classifier.Classify(expenses);
            base.Add(expenses);
        }

        public void Classify()
        {
            var msgs = this.repo.GetAll().ToList();
            this.Classifier.Classify(msgs);
            this.repo.Update(msgs);
        }

        public Dictionary<DateTime, Dictionary<string, IEnumerable<Transaction>>> GetExpensesByCategoriesByMonths(DateTime fromDate, DateTime toDate)
        {
            var expenses = this.repo.GetAll().Where(x => x.Date >= fromDate && x.Date <= toDate && x.Type == TransactionType.Expense);
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

        private readonly IUnitOfWork data;
        private TransactionsClassifier _classifier;
    }
}