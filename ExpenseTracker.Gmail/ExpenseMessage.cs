using System;

namespace ExpenseTracker.GmailConnector
{
    public class ExpenseMessage
    {
        public string Body { get; set; }
        public string Subject { get; set; }
        public DateTime EmailDate { get; set; }
    }
}