using System;
using System.Globalization;
using System.IO;
using ExpenseTracker.Core;

namespace ExpenseTracker.Allianz
{
    public class TransactionImporter : ITransactionImporter
    {
        private readonly TransactionsClassifier classifier = new TransactionsClassifier();
        private readonly IBaseDataItemService<Category> categoriesRepo;

        public TransactionImporter(IBaseDataItemService<Category> categoriesRepo)
        {
            this.categoriesRepo = categoriesRepo;
        }

        public Transaction Import(decimal amount, string details, TransactionType transactionType, DateTime date)
        {
            if (date == default(DateTime))
                throw new InvalidDataException("The transaction could not have its date processed correctly.");
            if (amount < 0)
                throw new InvalidDataException("The transaction could not have its amount processed correctly.");

            var t = new Transaction()
            {
                Amount = amount,
                Details = details.RemoveRepeatingSpaces(),
                Type = transactionType,
                Date = date
            };

            this.classifier.Classify(new Transaction[] { t }, this.categoriesRepo.GetAll());
            
            return t;
        }
    }
}
