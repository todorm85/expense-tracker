using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class CategoriesService
    {
        public CategoriesService(IUnitOfWork uow)
        {
            this.categories = uow.GetDataItemsRepo<Category>();
        }

        public void Insert(IEnumerable<Category> items)
        {
            var allCats = this.categories.GetAll();
            foreach (var item in items)
            {
                var existingCategory = allCats.FirstOrDefault(c => c.ExpenseSourcePhrase == item.ExpenseSourcePhrase);
                if (existingCategory != null)
                {
                    this.categories.Remove(existingCategory);
                }
            }

            this.categories.Insert(items);
        }

        public IEnumerable<Category> GetAll()
        {
            return this.categories.GetAll();
        }

        private IUnitOfWork uow;
        private IGenericRepository<Category> categories;

        public void Delete(string phrase)
        {
            var cat = this.categories.GetAll().FirstOrDefault(x => x.ExpenseSourcePhrase == phrase);
            if (cat != null)
            {
                this.categories.Remove(cat);
            }
            else
            {
                throw new InvalidOperationException($"Category with source phrase to match {phrase} does not exist.");
            }
        }
    }
}