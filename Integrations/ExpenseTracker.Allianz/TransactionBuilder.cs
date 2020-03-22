using System.Globalization;
using ExpenseTracker.Core;

namespace ExpenseTracker.Allianz
{
    public class TransactionBuilder : ITransactionBuilder
    {
        private readonly TransactionsClassifier classifier = new TransactionsClassifier();
        private readonly IBaseDataItemService<Category> categoriesRepo;

        public TransactionBuilder(IBaseDataItemService<Category> categoriesRepo)
        {
            this.categoriesRepo = categoriesRepo;
        }

        public void Build(Transaction t)
        {
            // nothing unique in transactions from mail
            if (string.IsNullOrEmpty(t.TransactionId))
            {
                t.TransactionId = $"{t.Date.ToString("dd_MM_yy", CultureInfo.InvariantCulture)}_{t.Amount.ToString("F2")}";
            }

            this.classifier.Classify(new Transaction[] { t }, this.categoriesRepo.GetAll());
            t.Details = t.Details.RemoveRepeatingSpaces();
        }
    }
}
