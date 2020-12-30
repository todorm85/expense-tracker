using ExpenseTracker.Core;
using ExpenseTracker.Core.Categories;
using ExpenseTracker.Core.Data;
using ExpenseTracker.Core.Transactions;
using LiteDB;
using System;
using System.Collections.Concurrent;
using System.Globalization;

namespace ExpenseTracker.Data
{
    public class UnitOfWork : IUnitOfWork
    {
        private readonly LiteDatabase db;

        private readonly ConcurrentDictionary<Type, object> repos = new ConcurrentDictionary<Type, object>();

        public UnitOfWork(string dbPath)
        {
            this.db = new LiteDatabase(dbPath);
            MapDbModels();
            this.HandleMigration();
        }

        public void Dispose()
        {
            this.db.Dispose();
        }

        public IGenericRepository<T> GetDataItemsRepo<T>() where T : class
        {
            if (this.repos.ContainsKey(typeof(T)))
            {
                return this.repos[typeof(T)] as IGenericRepository<T>;
            }

            var type = typeof(GenericRepo<T>);

            var ctor = type.GetConstructor(new Type[] { typeof(LiteDatabase), typeof(string) });
            GenericRepo<T> repoInstance = default(GenericRepo<T>);
            if (ctor != null)
            {
                repoInstance = ctor.Invoke(new object[] { this.db, this.GetSetName<T>() }) as GenericRepo<T>;
            }

            return this.repos.GetOrAdd(typeof(T), repoInstance) as IGenericRepository<T>;
        }

        private static void MapDbModels()
        {
            BsonMapper.Global.Entity<Transaction>()
                            .Id(x => x.TransactionId, false);
        }

        private string GetSetName<T>() where T : class
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

        private void HandleMigration()
        {
            if (this.db.Engine.UserVersion == 0)
            {
                var col = this.db.GetCollection(this.GetSetName<Transaction>());
                foreach (var doc in col.FindAll())
                {
                    // rename property
                    doc["Details"] = doc["Source"];
                    doc.Remove("Source");

                    // generate new transaction ids
                    var date = doc["Date"].AsDateTime;
                    var amnt = doc["Amount"].AsDecimal;
                    doc["TransactionId"] = $"{date.ToString("dd_MM_yy", CultureInfo.InvariantCulture)}_{amnt}";

                    // remove property
                    doc.Remove("Account");

                    col.Update(doc);
                }

                col = this.db.GetCollection(this.GetSetName<Category>());
                foreach (var doc in col.FindAll())
                {
                    doc["KeyWord"] = doc["ExpenseSourcePhrase"];
                    doc.Remove("ExpenseSourcePhrase");
                    col.Update(doc);
                }

                this.db.Engine.UserVersion = 1;
            }

            if (this.db.Engine.UserVersion == 1)
            {
                var col = this.db.GetCollection(this.GetSetName<Transaction>());
                foreach (var doc in col.FindAll())
                {
                    var originalId = doc["_id"];
                    var tId = (string)doc["TransactionId"];
                    if (tId != null && tId.Contains('/') && tId.Contains(':') && tId.Contains('_'))
                    {
                        doc["_id"] = TransactionsService.GenerateTransactionId(doc["Date"], doc["Amount"], doc["Details"]);
                    }
                    else if (tId != null)
                    {
                        doc["_id"] = tId;
                    }
                    else
                    {
                        doc["_id"] = originalId.ToString();
                    }

                    doc.Remove("TransactionId");
                    col.Insert(doc);
                    col.Delete(originalId);
                }

                this.db.Engine.UserVersion = 2;
            }
        }
    }
}