using ExpenseTracker.Allianz;
using System;

namespace ExpenseTracker.Tests.Common
{
    public abstract class MessageFactoryBase
    {
        private RandomGenerator rand = new RandomGenerator();

        public MessageFactoryBase()
        {
            Seed();
        }

        public string Amount { get; set; }
        public string Date { get; set; }
        public string Location { get; set; }

        public virtual ExpenseMessage GetInValidMessage(string date, string amount, string location)
        {
            this.Date = date;
            this.Amount = amount;
            this.Location = location;
            return this.GetInValidMessage();
        }

        public abstract ExpenseMessage GetInValidMessage();

        public ExpenseMessage GetRandomInValidMessage()
        {
            Seed();
            return this.GetInValidMessage();
        }

        public ExpenseMessage GetRandomValidMessage()
        {
            Seed();
            return this.GetValidMessage();
        }

        public virtual ExpenseMessage GetValidMessage(string date, string amount, string location)
        {
            this.Date = date;
            this.Amount = amount;
            this.Location = location;
            return this.GetValidMessage();
        }

        public abstract ExpenseMessage GetValidMessage();

        protected virtual void Seed()
        {
            this.Date = this.rand.GetDate();
            this.Amount = $"{this.rand.GetRandom(1, 10000)}.{this.rand.GetRandom(1, 250)}";
            this.Location = Guid.NewGuid().ToString();
        }
    }
}