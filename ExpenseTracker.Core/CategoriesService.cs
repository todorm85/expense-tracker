using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class CategoriesService
    {
        public CategoriesService(IUnitOfWork uow)
        {
            this.uow = uow;
        }

        public void Insert(IEnumerable<Category> items)
        {
            var allCats = this.uow.Categories.GetAll();
            foreach (var item in items)
            {
                var existingCategory = allCats.FirstOrDefault(c => c.ExpenseSourcePhrase == item.ExpenseSourcePhrase);
                if (existingCategory != null)
                {
                    this.uow.Categories.Remove(existingCategory);
                }
            }

            this.uow.Categories.Insert(items);
        }

        public IEnumerable<Category> GetAll()
        {
            return this.uow.Categories.GetAll();
        }

        private IUnitOfWork uow;

        public void Delete(string phrase)
        {
            var cat = this.uow.Categories.GetAll().FirstOrDefault(x => x.ExpenseSourcePhrase == phrase);
            if (cat != null)
            {
                this.uow.Categories.Remove(cat);
            }
            else
            {
                throw new InvalidOperationException($"Category with source phrase to match {phrase} does not exist.");
            }
        }
    }
}