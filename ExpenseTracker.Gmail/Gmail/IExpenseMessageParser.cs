using System.Collections.Generic;
using ExpenseTracker.Core;

namespace ExpenseTracker.Allianz
{
    public interface IExpenseMessageParser
    {
        IEnumerable<Transaction> Parse(List<ExpenseMessage> expenseMessages);
    }
}