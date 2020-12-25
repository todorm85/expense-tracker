using System;

namespace ExpenseTracker.Allianz.Gmail
{
    public interface IMailClient : IDisposable
    {
        int Count { get; }

        ExpenseMessage GetMessage(int i);
        
        void Delete(int msgIdx);
        void MarkRead(int msgIdx);
    }
}
