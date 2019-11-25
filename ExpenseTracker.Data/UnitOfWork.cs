using System;
using System.Collections.Generic;
using ExpenseTracker.Core;
using LiteDB;

namespace ExpenseTracker.Data
{
    public class UnitOfWork : IUnitOfWork, IDisposable
    {
        public UnitOfWork(string dbPath)
        {
            this.db = new LiteDatabase(dbPath);
            this.HandleMigration();
        }

        private void HandleMigration()
        {
            if (this.db.Engine.UserVersion == 0)
            {
                var col = this.db.GetCollection(this.GetSetName<Transaction>());
                foreach (var doc in col.FindAll())
                {
                    doc["Details"] = doc["Source"];
                    doc.Remove("Source");
                    doc.Remove("TransactionId");
                    doc.Remove("Account");
                    col.Update(doc);
                }

                this.db.Engine.UserVersion = 1;
            }
        }

        public IGenericRepository<T> GetDataItemsRepo<T>() where T : IDataItem
        {
            if (this.repos.ContainsKey(typeof(T)))
            {
                return this.repos[typeof(T)] as IGenericRepository<T>;
            }

            var type = typeof(GenericRepo<T>);

            var ctor = type.GetConstructor(new Type[] { typeof(LiteDatabase), typeof(string) });
            GenericRepo<T> obj = default(GenericRepo<T>);
            if (ctor != null)
            {
                obj = ctor.Invoke(new object[] { this.db, this.GetSetName<T>() }) as GenericRepo<T>;
            }

            this.repos.Add(typeof(T), obj);

            return obj;
        }

        public void Dispose()
        {
            this.db.Dispose();
        }

        private string GetSetName<T>() where T : IDataItem
        {
            // backward compatability
            if (typeof(T) == typeof(Category))
            {
                return "categories";
            }
            else
            {
                return $"{typeof(T).Name.ToLower()}s";
            }
        }

        private readonly LiteDatabase db;
        private readonly IDictionary<Type, object> repos = new Dictionary<Type, object>();
    }
}