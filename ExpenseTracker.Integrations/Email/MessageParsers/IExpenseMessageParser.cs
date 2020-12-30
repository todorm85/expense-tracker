using ExpenseTracker.Core.Transactions;

namespace ExpenseTracker.Allianz
{
    public interface IExpenseMessageParser
    {
        Transaction Parse(ExpenseMessage expenseMessages);
    }
}