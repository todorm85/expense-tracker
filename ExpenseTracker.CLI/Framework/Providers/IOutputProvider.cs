namespace ExpenseTracker.UI
{
    public interface IOutputProvider
    {
        Style Style { get; set; }

        void NewLine();

        void Write(string value);
    }
}