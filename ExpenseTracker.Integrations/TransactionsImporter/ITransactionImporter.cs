using System;
using ExpenseTracker.Core;

namespace ExpenseTracker.Allianz
{
    public interface ITransactionImporter
    {
        Transaction Import(decimal amount, string details, TransactionType transactionType, DateTime date);
    }
}