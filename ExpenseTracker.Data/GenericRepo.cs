using System;
using System.Collections.Generic;
using System.Linq;
using ExpenseTracker.Core;
using LiteDB;

namespace ExpenseTracker.Data
{
    public class GenericRepo<T> : IGenericRepository<T>
        where T : IDataItem
    {
        private LiteDatabase db;
        private LiteCollection<T> context;

        public GenericRepo(LiteDatabase db, string collection)
        {
            this.db = db;
            this.context = this.db.GetCollection<T>(collection);
        }

        public virtual IEnumerable<T> GetAll()
        {
            var all = this.context.FindAll();
            return all;
        }

        public virtual void Insert(IEnumerable<T> items)
        {
            this.context.Insert(items);
        }

        public virtual void Update(IEnumerable<T> items)
        {
            var allExistingIds = this.GetAll().Select(x => x.Id);
            foreach (var item in items)
            {
                if (item.Id == 0 || !allExistingIds.Contains(item.Id))
                {
                    throw new InvalidOperationException($"Cannot update item with id {item.Id}. Item not found in database.");
                }
            }

            this.context.Update(items);
        }

        public virtual void Remove(IEnumerable<T> items)
        {
            foreach (var item in items)
            {
                this.context.Delete(item.Id);
            }
        }
    }
}
