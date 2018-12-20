using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class ExpensesService
    {
        public ExpensesService(IExpensesRepository repo)
            :this(repo, new Dictionary<string, string>())
        {

        }

        public ExpensesService(IExpensesRepository repo, IDictionary<string, string> keysCategories)
        {
            this.repo = repo;
            this.classifier = new ExpensesClassifier(keysCategories);
        }

        public IDictionary<string, string> KeysCategories { get; set; }

        public void Add(IEnumerable<Expense> expenses)
        {
            var allExisting = this.repo.GetAll().Select(x => x.TransactionId);
            expenses = expenses.Where(x => !allExisting.Contains(x.TransactionId));

            this.classifier.Classify(expenses);
            this.repo.Insert(expenses);
        }
        
        public void Classify()
        {
            var msgs = this.repo.GetAll().ToList();
            this.classifier.Classify(msgs);
            this.repo.Update(msgs);
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

        private IExpensesRepository repo;
        private ExpensesClassifier classifier;
    }
}