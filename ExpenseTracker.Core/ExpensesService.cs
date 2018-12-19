using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class ExpensesService
    {
        public ExpensesService(IEnumerable<IExpensesImporter> importers, IEnumerable<IExpensesExporter> exporters, IExpensesRepository repo)
        {
            this.importers = importers;
            this.exporters = exporters;
            this.repo = repo;
            this.KeysCategories = new Dictionary<string, string>();
        }

        public IDictionary<string, string> KeysCategories { get; set; }

        public void Import()
        {
            foreach (var importer in this.importers)
            {
                var msgs = importer.Import();

                var allExisting = this.repo.GetAll().Select(x => x.TransactionId);
                msgs = msgs.Where(x => !allExisting.Contains(x.TransactionId));

                this.GetClassifier().Classify(msgs);
                this.repo.Insert(msgs);
            }
        }

        public void Classify()
        {
            var msgs = this.repo.GetAll().ToList();
            this.GetClassifier().Classify(msgs);
            this.repo.Update(msgs);
        }

        public void ExportByMonths(DateTime fromDate, DateTime toDate, bool detailed = true)
        {
            var expenses = this.repo.GetAll().Where(x => x.Date >= fromDate && x.Date <= toDate);
            var expensesByMonth = GetByMonths(expenses);

            if (!detailed)
            {
                var categoriesByMonth = new Dictionary<DateTime, Dictionary<string, decimal>>();
                foreach (var month in expensesByMonth)
                {
                    var cats = GetCategoryExpenses(month.Value);
                    categoriesByMonth.Add(month.Key, cats);
                }

                foreach (var exporter in this.exporters)
                {
                    exporter.Export(categoriesByMonth);
                }
            }
            else
            {
                foreach (var exporter in this.exporters)
                {
                    exporter.Export(expensesByMonth);
                }
            }
        }

        private static Dictionary<DateTime, IEnumerable<Expense>> GetByMonths(IEnumerable<Expense> expenses)
        {
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

        private static Dictionary<string, decimal> GetCategoryExpenses(IEnumerable<Expense> expenses)
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

        private ExpensesClassifier GetClassifier()
        {
            return new ExpensesClassifier(this.KeysCategories);
        }

        private IEnumerable<IExpensesImporter> importers;
        private IEnumerable<IExpensesExporter> exporters;
        private IExpensesRepository repo;
    }
}