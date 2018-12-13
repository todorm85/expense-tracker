using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Core
{
    public class ExpensesService
    {
        private IExpensesMessagesClient messageClient;
        private IExpensesRepository repo;
        private ExpensesClassifier classifier;

        public ExpensesService(IExpensesMessagesClient client, IExpensesRepository repo, ExpensesClassifier classifier)
        {
            this.messageClient = client;
            this.repo = repo;
            this.classifier = classifier;
        }

        public void Import()
        {
            var msgs = messageClient.ReadAll();
            classifier.Classify(msgs);
            repo.Insert(msgs);
        }

        public void Classify()
        {
            var msgs = repo.GetAll();
            classifier.Classify(msgs);
            repo.Update(msgs);
        }
    }
}
