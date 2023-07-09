namespace ExpenseTracker.App
{
    public class Config
    {
        public string DbPath { get; set; }
        public string MailPass { get; set; }
        public string MailUser { get; set; }
        public string UserHashedPass { get; set; }
        public int LockoutMinutes { get; set; }
    }
}