using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Integrations.ApiClients.Trading212
{
    public class Merchant
    {
        public string Name { get; set; }
        public string Category { get; set; }
        public string Address { get; set; }
        public string CountryCode { get; set; }
    }
}
