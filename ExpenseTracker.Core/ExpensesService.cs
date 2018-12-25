using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class ExpensesService : IDataItemService<Expense>
    {
        public ExpensesService(IUnitOfWork data)
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

        public void Add(IEnumerable<Expense> expenses)
        {
            var allExisting = this.data.GetDataItemsRepo<Expense>().GetAll().Select(x => x.TransactionId);
            expenses = expenses.Where(x => !allExisting.Contains(x.TransactionId));

            this.Classifier.Classify(expenses);
            this.data.GetDataItemsRepo<Expense>().Insert(expenses);
        }

        public void Classify()
        {
            var msgs = this.data.GetDataItemsRepo<Expense>().GetAll().ToList();
            this.Classifier.Classify(msgs);
            this.data.GetDataItemsRepo<Expense>().Update(msgs);
        }

        public Dictionary<DateTime, Dictionary<string, IEnumerable<Expense>>> GetExpensesByCategoriesByMonths(DateTime fromDate, DateTime toDate)
        {
            var expenses = this.data.GetDataItemsRepo<Expense>().GetAll().Where(x => x.Date >= fromDate && x.Date <= toDate);
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
            var expenses = this.data.GetDataItemsRepo<Expense>().GetAll().Where(x => x.Date >= fromDate && x.Date <= toDate);
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

        public IEnumerable<Expense> GetAll()
        {
            return this.data.GetDataItemsRepo<Expense>().GetAll();
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

        public void Remove(IEnumerable<Expense> items)
        {
            throw new NotImplementedException();
        }

        public void Update(IEnumerable<Expense> items)
        {
            this.data.GetDataItemsRepo<Expense>().Update(items);
        }

        private IUnitOfWork data;
        private ExpensesClassifier _classifier;
    }
}