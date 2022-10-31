using System.Collections.Generic;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Rules;
using ExpenseTracker.Core.Services.Models;
using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Core.Services
{
    public interface IExpensesService : IReadRepository<Transaction>, IReadRepository<Rule>
    {
        bool TryCreateTransactions(IEnumerable<Transaction> expenses, out IEnumerable<CreateTransactionResult> skipped);

        bool TryCreateTransaction(Transaction t, out IEnumerable<CreateTransactionResult> skipped);

        void RemoveTransaction(string id);

        void UpdateTransactions(IEnumerable<Transaction> transactions);

        void UpdateTransaction(Transaction transaction);

        void RemoveRule(int id);

        void CreateRule(Rule createRuleModel);
        
        void UpdateRule(Rule model);

        List<List<Transaction>> GetPotentialDuplicates();

        void RenameCategory(string oldName, string newName);

        IEnumerable<string> GetAllCategories();

        void ProcessAllUncategorizedTransactions();
    }
}