using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ExpenseTracker.Core
{
    public interface IUnitOfWork
    {
        IGenericRepository<Expense> Expenses { get; }
        IGenericRepository<Category> Categories { get; }
        IGenericRepository<Budget> Budgets { get; }
    }
}
