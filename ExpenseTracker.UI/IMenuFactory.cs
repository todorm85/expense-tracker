namespace ExpenseTracker.UI
{
    public interface IMenuFactory
    {
        T Get<T>() where T : MenuBase;
    }
}