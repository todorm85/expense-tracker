using System.Collections.Generic;
using System.Linq;

namespace ExpenseTracker.Core
{
    public class ExpensesService
    {
        public ExpensesService(IExpensesMessagesClient client, IExpensesRepository repo)
        {
            this.messageClient = client;
            this.repo = repo;
            this.KeysCategories = new Dictionary<string, string>();
        }

        public IDictionary<string, string> KeysCategories { get; set; }

        public void Import()
        {
            var msgs = this.messageClient.ReadAll();

            var allExisting = repo.GetAll().Select(x => x.TransactionId);
            msgs = msgs.Where(x => !allExisting.Contains(x.TransactionId));

            this.GetClassifier().Classify(msgs);
            this.repo.Insert(msgs);
        }

        public void Classify()
        {
            var msgs = this.repo.GetAll();
            this.GetClassifier().Classify(msgs);
            this.repo.Update(msgs);
        }

        private ExpensesClassifier GetClassifier()
        {
            return new ExpensesClassifier(this.KeysCategories);
        }

        private IExpensesMessagesClient messageClient;
        private IExpensesRepository repo;
    }
}