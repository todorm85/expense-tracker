namespace ExpenseTracker.Core
{
    public class Category : IDataItem
    {
        public string Name { get; set; }

        public string ExpenseSourcePhrase { get; set; }

        public int Id { get; set; }
    }
}