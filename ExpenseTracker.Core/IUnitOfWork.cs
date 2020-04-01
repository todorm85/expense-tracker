using System;

namespace ExpenseTracker.Core
{
    public interface IUnitOfWork : IDisposable
    {
        IGenericRepository<T> GetDataItemsRepo<T>() where T : IDataItem;
    }
}
