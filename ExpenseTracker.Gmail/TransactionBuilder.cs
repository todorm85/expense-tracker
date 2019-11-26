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
            t.TransactionId = $"{t.Date.ToString("dd_MM_yy", CultureInfo.InvariantCulture)}_{t.Amount.ToString("F2")}";
            this.classifier.Classify(new Transaction[] { t }, this.categoriesRepo.GetAll());
        }
    }
}
