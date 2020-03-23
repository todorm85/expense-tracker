namespace ExpenseTracker.App
{
    public interface IConfig
    {
        public string DbPath { get; set; }
        public string MailUser { get; set; }
        public string MailPass { get; set; }
    }
}