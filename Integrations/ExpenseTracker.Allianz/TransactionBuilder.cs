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
            // no way to reliably distignuish between transactions imported from file and ones from mail
            // assuming transactions for a day are imported at once - then even if there are collisions between transactions for that day they will be inserted as tehere would still be nothing for that day in the DB
            // if same day tries to get updated again then the batch insert operation will detect there are existing entries in the DB
            t.TransactionId = $"{t.Date.ToString("dd_MM_yy", CultureInfo.InvariantCulture)}_{t.Amount.ToString("F2")}";
            this.classifier.Classify(new Transaction[] { t }, this.categoriesRepo.GetAll());
            t.Details = t.Details.RemoveRepeatingSpaces();
        }
    }
}
