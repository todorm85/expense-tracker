using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class CategoriesService : BaseDataItemService<Category>
    {
        public CategoriesService(IUnitOfWork uow) : base(uow)
        {
        }

        public override void Add(IEnumerable<Category> items)
        {
            var allCats = this.repo.GetAll();
            foreach (var item in items)
            {
                var existingCategory = allCats.FirstOrDefault(c => c.ExpenseSourcePhrase == item.ExpenseSourcePhrase);
                if (existingCategory != null)
                {
                    this.Remove(new Category[] { existingCategory });
                }
            }

            base.Add(items);
        }
    }
}