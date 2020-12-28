using System;
using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class CategoriesService : BaseDataItemService<Category>, IBaseDataItemService<Category>
    {
        public CategoriesService(IUnitOfWork uow) : base(uow)
        {
        }

        public override void Add(IEnumerable<Category> items)
        {
            foreach (var item in items)
            {
                var existingCategory = this.repo.GetAll(c => c.KeyWord == item.KeyWord).FirstOrDefault();
                if (existingCategory != null)
                {
                    this.RemoveById(new Category[] { existingCategory });
                }
            }

            base.Add(items);
        }
    }
}