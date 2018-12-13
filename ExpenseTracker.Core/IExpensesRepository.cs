using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Core
{
    public interface IExpensesRepository
    {
        IEnumerable<Expense> GetAll();
        void Insert(IEnumerable<Expense> expenses);
        void Update(IEnumerable<Expense> expenses);
    }
}
