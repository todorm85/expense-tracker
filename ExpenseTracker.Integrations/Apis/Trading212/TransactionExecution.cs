using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.Integrations.ApiClients.Trading212
{
    public class TransactionExecution
    {
        public int Id { get; set; }
        public string TransactionToken { get; set; }
        public int CardId { get; set; }
        public int AccountId { get; set; }
        public string ClientReferenceId { get; set; }
        public decimal Amount { get; set; }
        public decimal? ExecutionAmount { get; set; }
        public string CurrencyCode { get; set; }
        public string Status { get; set; }
        public DateTime TimeCreated { get; set; }
        public DateTime TimeUpdated { get; set; }
        public DateTime? ExpectedReversalDate { get; set; }
        public Merchant Merchant { get; set; }
        public bool IsRecurring { get; set; }
        public string PaymentChannel { get; set; }
        public string StatusReason { get; set; }
        public string Type { get; set; }
        public string CardLastFour { get; set; }
        public object CurrencyConversion { get; set; }
        public object AtmWithdrawalFee { get; set; }
        public object Cashback { get; set; }
        public object StatusReasonParams { get; set; }
        public bool CanGenerateStatement { get; set; }
        public object RoundUp { get; set; }
    }
}
