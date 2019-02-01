namespace ExpenseTracker.UI
{
    public interface IOutputRenderer
    {
        void Write(string value, Style style);

        void Write(string value);

        void WriteLine();

        void WriteLine(string value);

        string PromptInput(string msg, string defaultValue = "");

        void WriteLine(string value, Style style);

        bool Confirm(string msg = "Confirm? y/n: ");
    }
}