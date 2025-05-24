using System;

namespace ExpenseTracker.Allianz.Gmail
{
    public interface IMailClient : IDisposable
    {
        int Count { get; }

        void Delete(int msgIdx);

        ExpenseMessage GetMessage(int i);

        bool TestConnection();
    }
}