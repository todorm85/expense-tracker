namespace ExpenseTracker.App
{
    public class Config : IConfig
    {
        public string DbPath { get; set; }
        public string MailUser { get; set; }
        public string MailPass { get; set; }
    }
}