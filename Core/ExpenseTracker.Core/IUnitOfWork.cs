namespace ExpenseTracker.Core
{
    public interface IUnitOfWork
    {
        IGenericRepository<T> GetDataItemsRepo<T>() where T : IDataItem;
    }
}
