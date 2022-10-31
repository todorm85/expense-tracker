using System;

namespace ExpenseTracker.Core.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IRepository<T> GetDataItemsRepo<T>() where T : class;
    }
}