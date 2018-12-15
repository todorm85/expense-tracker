using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Core
{
    public interface IExpensesImporter
    {
        IEnumerable<Expense> Import();
    }
}
