using System;

namespace ExpenseTracker.Allianz
{
    public class ExpenseMessage
    {
        public string Body { get; set; }
        public string Subject { get; set; }
        public DateTime EmailDate { get; set; }
    }
}