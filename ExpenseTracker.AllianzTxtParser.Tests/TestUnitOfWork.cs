using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using ExpenseTracker.Allianz;
using ExpenseTracker.Core;

namespace ExpenseTracker.AllianzTxtParser.Tests
{
    internal class TestUnitOfWork : IUnitOfWork
    {
        public IGenericRepository<T> GetDataItemsRepo<T>() where T : IDataItem
        {
            if (typeof(T) == typeof(Category))
            {
                return new TestRepo<T>(Categories as IEnumerable<T>);
            }

            return new TestRepo<T>(new List<IDataItem>() as IEnumerable<T>);
        }

        public IEnumerable<Category> Categories { get; set; }
    }

    internal class TestRepo<TCat> : IGenericRepository<TCat> where TCat : IDataItem
    {
        private IEnumerable<TCat> categories;

        public TestRepo(IEnumerable<TCat> categories)
        {
            this.categories = categories;
        }

        public IEnumerable<TCat> GetAll()
        {
            return this.categories as IEnumerable<TCat>;
        }

        public IEnumerable<TCat> GetAll(Expression<Func<TCat, bool>> predicate)
        {
            return this.categories.Where(predicate.Compile(true));
        }

        public void Insert(IEnumerable<TCat> items)
        {
            throw new System.NotImplementedException();
        }

        public void Remove(IEnumerable<TCat> items)
        {
            throw new System.NotImplementedException();
        }

        public void Update(IEnumerable<TCat> items)
        {
            throw new System.NotImplementedException();
        }
    }
}