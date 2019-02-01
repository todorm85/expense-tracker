using ExpenseTracker.Core;
using System;
using System.Collections.Generic;
using System.Text;

namespace ExpenseTracker.UI
{
    internal class Serializer
    {
        public string Serialize(object value)
        {
            if (value == null)
            {
                return null;
            }

            var type = value.GetType();
            if (type == typeof(Dictionary<string, decimal>))
            {
                var sb = new StringBuilder();
                foreach (var ce in value as Dictionary<string, decimal>)
                {
                    sb.Append($"{ce.Key}:{ce.Value};");
                }

                return sb.ToString();
            }
            else if (typeof(IEnumerable<Transaction>).IsAssignableFrom(type))
            {
                var sb = new StringBuilder();
                foreach (var t in value as IEnumerable<Transaction>)
                {
                    sb.Append($"{t.Category}:{t.Source}:{this.Serialize(t.Type)}:{t.Amount};");
                }

                return sb.ToString();
            }
            else if (type == typeof(DateTime))
            {
                var dateValue = (DateTime)value;
                return dateValue.ToShortDateString();
            }
            else if (type == typeof(TransactionType))
            {
                if ((TransactionType)value == TransactionType.Expense)
                {
                    return "-";
                }
                else
                {
                    return "+";
                }
            }
            else
            {
                return value?.ToString();
            }
        }

        public object Deserialize(Type propertyType, string newValue)
        {
            if (propertyType == typeof(string))
            {
                return newValue;
            }
            else if (propertyType == typeof(decimal))
            {
                return decimal.Parse(newValue);
            }
            else if (propertyType == typeof(DateTime))
            {
                return DateTime.Parse(newValue);
            }
            else if (propertyType == typeof(Dictionary<string, decimal>))
            {
                var dictionary = new Dictionary<string, decimal>();
                var kvs = newValue.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var kv in kvs)
                {
                    dictionary.Add(kv.Split(':')[0], decimal.Parse(kv.Split(':')[1]));
                }

                return dictionary;
            }
            else if (typeof(IEnumerable<Transaction>).IsAssignableFrom(propertyType))
            {
                var transactions = new List<Transaction>();
                var serializedTransactions = newValue.Split(new string[] { ";" }, StringSplitOptions.RemoveEmptyEntries);
                foreach (var serTran in serializedTransactions)
                {
                    var vals = serTran.Split(':');
                    transactions.Add(new Transaction()
                    {
                        Category = vals[0],
                        Source = vals[1],
                        Type = (TransactionType)this.Deserialize(typeof(TransactionType), vals[2]),
                        Amount = decimal.Parse(vals[3])
                    });
                }

                return transactions;
            }
            else if (propertyType == typeof(TransactionType))
            {
                if (newValue == "+")
                {
                    return TransactionType.Income;
                }
                else
                {
                    return TransactionType.Expense;
                }
            }
            else
            {
                throw new ArgumentException("Unknown property type to edit");
            }
        }
    }
}
