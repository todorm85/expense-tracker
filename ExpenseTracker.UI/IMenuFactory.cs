using System;

namespace ExpenseTracker.UI
{
    public interface IMenuFactory
    {
        T Create<T>(Type type) where T: Menu;
    }
}