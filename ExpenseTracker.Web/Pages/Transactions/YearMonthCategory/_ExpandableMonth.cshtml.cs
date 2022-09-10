using ExpenseTracker.Core.Transactions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Web.Pages.Transactions
{
    public class ExpandableMonthModel : IEnumerable<ExpandableCategoryModel>
    {
        private const string UnspecifiedCategoryKeyName = "unspecified";
        private decimal? totalExpenses;
        private decimal? totalIncome;
        private IList<ExpandableCategoryModel> categories = new List<ExpandableCategoryModel>();

        public ExpandableMonthModel(DateTime month)
        {
            this.Month = month;
        }

        public decimal Balance => this.TotalIncome - this.TotalExpenses;
        public int Count => this.categories.Count;
        public DateTime Month { get; private set; }

        public decimal TotalExpenses
        {
            get
            {
                if (!this.totalExpenses.HasValue)
                    this.totalExpenses = this.categories.Sum(x => x.TotalExpense);
                return this.totalExpenses.Value;
            }
        }

        public decimal TotalIncome
        {
            get
            {
                if (!this.totalIncome.HasValue)
                    this.totalIncome = this.categories.Sum(x => x.TotalIncome);
                return this.totalIncome.Value;
            }
        }

        public ExpandableCategoryModel this[int index] { get => this.categories[index]; set => this.categories[index] = value; }

        public IEnumerator<ExpandableCategoryModel> GetEnumerator()
        {
            foreach (var item in this.categories)
            {
                yield return item;
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        public void OrderMonthCategories()
        {
            foreach (var ca in this.categories)
            {
                OrderCategory(ca);
            }

            this.categories = this.categories.OrderByDescending(x => x.TotalExpense).ToList();
        }

        private void OrderCategory(ExpandableCategoryModel category)
        {
            category.Categories = category.Categories.OrderByDescending(x => x.TotalExpense).ToList();
            category.OrderTransactions();
            foreach (var childCategory in category.Categories)
            {
                OrderCategory(childCategory);
            }
        }

        public static string GetCategoryKey(string currentCategory)
        {
            currentCategory = string.IsNullOrEmpty(currentCategory) ? UnspecifiedCategoryKeyName : currentCategory;
            return currentCategory;
        }

        internal void AddTransaction(Transaction t)
        {
            var categoryParts = t.Category?.Split("/") ?? new string[1] { null };
            var currentCategoriesGroup = this.categories;
            var currentCategoryName = string.Empty;
            for (int i = 0; i < categoryParts.Length; i++)
            {
                currentCategoryName = (currentCategoryName + "/" + categoryParts[i]).Trim('/');
                var category = currentCategoriesGroup.FirstOrDefault(x => x.CategoryName == currentCategoryName);
                if (category == null)
                {
                    category = new ExpandableCategoryModel(currentCategoryName);
                    currentCategoriesGroup.Add(category);
                }

                if (t.Type == TransactionType.Expense)
                {
                    category.TotalExpense += t.Amount;
                }
                else if (t.Type == TransactionType.Income)
                {
                    category.TotalIncome += t.Amount;
                }

                if (i == categoryParts.Length - 1)
                {
                    category.Add(t);
                }

                currentCategoriesGroup = category.Categories;
            }
        }
    }
}
