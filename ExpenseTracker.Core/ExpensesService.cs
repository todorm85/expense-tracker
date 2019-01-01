using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class ExpensesService : BaseDataItemService<Expense>, IExpensesService
    {
        public ExpensesService(IUnitOfWork data) : base(data)
        {
            this.data = data;
        }

        private ExpensesClassifier Classifier
        {
            get
            {
                if (this._classifier == null)
                {
                    this._classifier = new ExpensesClassifier(this.data.GetDataItemsRepo<Category>().GetAll());
                }

                return this._classifier;
            }
        }

        public override void Add(IEnumerable<Expense> expenses)
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

        public Dictionary<DateTime, Dictionary<string, IEnumerable<Expense>>> GetExpensesByCategoriesByMonths(DateTime fromDate, DateTime toDate)
        {
            var expenses = this.repo.GetAll().Where(x => x.Date >= fromDate && x.Date <= toDate);
            var byCategoryByMonths = new Dictionary<DateTime, Dictionary<string, IEnumerable<Expense>>>();
            foreach (var year in expenses.GroupBy(x => x.Date.Year))
            {
                foreach (var month in year.GroupBy(x => x.Date.Month))
                {
                    var categories = new Dictionary<string, IEnumerable<Expense>>();
                    foreach (var cat in month.GroupBy(x => x.Category))
                    {
                        categories.Add(cat.Key ?? "", cat.ToList());
                    }

                    byCategoryByMonths.Add(new DateTime(year.Key, month.Key, 1), categories);
                }
            }

            return byCategoryByMonths;
        }

        public void Update(Expense expense)
        {
            this.Update(new Expense[] { expense });
        }

        private readonly IUnitOfWork data;
        private ExpensesClassifier _classifier;
    }
}