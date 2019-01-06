namespace ExpenseTracker.ConsoleClient
{
    internal interface ISettings
    {
        string DbPath { get; }

        string MailUser { get; }

        string MailPass { get; }

        string ClientMode { get; }

        string WebClientAuth { get; }

        string WebServiceBase { get; }
    }
}