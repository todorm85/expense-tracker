using System;

namespace ExpenseTracker.Core.Data
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> GetDataItemsRepo<T>() where T : class;
    }
}