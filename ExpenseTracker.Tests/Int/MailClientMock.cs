﻿using ExpenseTracker.Allianz;
using ExpenseTracker.Allianz.Gmail;
using System.Collections.Generic;

namespace ExpenseTracker.Tests.Int
{
    public class MailClientMock : IMailClient
    {
        public MailClientMock()
        {
            this.MockedMessages = new List<ExpenseMessage>();
        }

        public int Count => this.MockedMessages.Count;
        public IList<ExpenseMessage> MockedMessages { get; set; }

        public void Delete(int msgIdx)
        {
            this.MockedMessages.RemoveAt(msgIdx);
        }

        public void Dispose()
        {
        }

        public ExpenseMessage GetMessage(int i)
        {
            return this.MockedMessages[i];
        }

        public bool TestConnection()
        {
            throw new System.NotImplementedException();
        }
    }
}