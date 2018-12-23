using System;
using ExpenseTracker.Core;
using ExpenseTracker.Data;

namespace ExpenseTracker.ConsoleClient
{
    internal class ServicesFactory
    {
        public static IUnitOfWork GetUnitOfWork()
        {
            string path = Utils.GetDbPath();

            return new UnitOfWork(path);
        }

        public static T GetService<T>() where T : class
        {
            var type = typeof(T);

            var ctor = type.GetConstructor(new Type[] { typeof(IUnitOfWork) });
            T obj = default(T);
            if (ctor != null)
            {
                obj = ctor.Invoke(new object[] { GetUnitOfWork() }) as T;
            }

            return obj;
        }
    }
}