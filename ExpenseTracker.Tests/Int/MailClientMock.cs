using System.Collections.Generic;
using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;

namespace ExpenseTracker.Tests.Int
{
    public class MailClientMock : IMailClient
    {
        public MailClientMock()
        {
            this.MockedMessages = new List<ExpenseMessage>();
        }

        public IList<ExpenseMessage> MockedMessages { get; set; }
        
        public int Count => this.MockedMessages.Count;

        public void Delete(int msgIdx)
        {
            this.MockedMessages.RemoveAt(msgIdx);
        }

        public void Dispose() { }

        public ExpenseMessage GetMessage(int i)
        {
            return this.MockedMessages[i];
        }

        public void MarkRead(int msgIdx)
        {
            throw new System.NotImplementedException();
        }
    }
}