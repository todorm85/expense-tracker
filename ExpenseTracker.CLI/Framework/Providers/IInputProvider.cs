namespace ExpenseTracker.UI
{
    public interface IInputProvider
    {
        string Read();
        string Read(string preenteredValue);
    }
}
