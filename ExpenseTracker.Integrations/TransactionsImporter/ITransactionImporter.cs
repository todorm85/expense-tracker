using ExpenseTracker.Core;
using System;

namespace ExpenseTracker.Allianz
{
    public interface ITransactionImporter
    {
        Transaction Import(decimal amount, string details, TransactionType transactionType, DateTime date);
    }
}