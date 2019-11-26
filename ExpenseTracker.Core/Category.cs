namespace ExpenseTracker.Core
{
    public class Category : IDataItem
    {
        public string Name { get; set; }

        public string KeyWord { get; set; }

        public int Id { get; set; }
    }
}