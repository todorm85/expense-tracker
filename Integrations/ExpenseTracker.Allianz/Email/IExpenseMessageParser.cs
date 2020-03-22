using ExpenseTracker.Core;

namespace ExpenseTracker.Allianz
{
    public interface IExpenseMessageParser
    {
        Transaction Parse(ExpenseMessage expenseMessages);
    }
}