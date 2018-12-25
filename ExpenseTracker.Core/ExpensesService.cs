using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class ExpensesService : BaseDataItemService<Expense>
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
            var allExisting = this.repo.GetAll().Select(x => x.TransactionId);
            expenses = expenses.Where(x => !allExisting.Contains(x.TransactionId));

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

        public Dictionary<DateTime, IEnumerable<Expense>> GetExpensesByMonths(DateTime fromDate, DateTime toDate)
        {
            var expenses = this.repo.GetAll().Where(x => x.Date >= fromDate && x.Date <= toDate);
            var byMonth = new Dictionary<DateTime, IEnumerable<Expense>>();
            foreach (var year in expenses.GroupBy(x => x.Date.Year))
            {
                foreach (var month in year.GroupBy(x => x.Date.Month))
                {
                    byMonth.Add(new DateTime(year.Key, month.Key, 1), month.ToList());
                }
            }

            return byMonth;
        }

        public void Update(Expense expense)
        {
            this.Update(new Expense[] { expense });
        }

        public Dictionary<DateTime, Dictionary<string, decimal>> GetCategoriesCostByMonth(DateTime fromDate, DateTime toDate)
        {
            var categoriesByMonth = new Dictionary<DateTime, Dictionary<string, decimal>>();
            var expensesByMonth = this.GetExpensesByMonths(fromDate, toDate);
            foreach (var month in expensesByMonth)
            {
                var cats = this.GetCategoriesCost(month.Value);
                categoriesByMonth.Add(month.Key, cats);
            }

            return categoriesByMonth;
        }

        private Dictionary<string, decimal> GetCategoriesCost(IEnumerable<Expense> expenses)
        {
            var categories = expenses.GroupBy(x => x.Category);
            var categoriesAmount = new Dictionary<string, decimal>();
            foreach (var category in categories)
            {
                var amount = category.Sum(x => x.Amount);
                categoriesAmount.Add(category.Key ?? "", amount);
            }

            return categoriesAmount;
        }

        private IUnitOfWork data;
        private ExpensesClassifier _classifier;
    }
}