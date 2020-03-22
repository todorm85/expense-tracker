namespace ExpenseTracker.UI
{
    public interface IOutputProvider
    {
        Style Style { get; set; }

        void Write(string value);

        void NewLine();
    }
}