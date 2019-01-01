using System;
using System.Collections.Generic;
using System.Reflection;
using System.Text;

namespace ExpenseTracker.UI
{
    internal class ItemEditor
    {
        public ItemEditor(object item)
        {
            this.item = item;
        }

        public string GetPropVal(PropertyInfo p)
        {
            var propertyType = p.PropertyType;
            if (propertyType == typeof(Dictionary<string, decimal>))
            {
                var catExpenditure = (Dictionary<string, decimal>)p.GetValue(this.item) ?? new Dictionary<string, decimal>();
                return this.Serialize(catExpenditure);
            }

            return p.GetValue(this.item)?.ToString();
        }

        public void SetPropVal(PropertyInfo p, string newValue)
        {
            var propertyType = p.PropertyType;
            if (propertyType == typeof(string))
            {
                p.SetValue(this.item, newValue);
            }
            else if (propertyType == typeof(decimal))
            {
                p.SetValue(this.item, decimal.Parse(newValue));
            }
            else if (propertyType == typeof(DateTime))
            {
                p.SetValue(this.item, DateTime.Parse(newValue));
            }
            else if (propertyType == typeof(Dictionary<string, decimal>))
            {
                p.SetValue(this.item, this.Deserialize(newValue));
            }
            else
            {
                throw new ArgumentException("Unknown property type to edit");
            }
        }

        public PropertyInfo[] GetProperties()
        {
            return this.item.GetType().GetProperties();
        }

        private string Serialize(Dictionary<string, decimal> dic)
        {
            var sb = new StringBuilder();

            foreach (var ce in dic)
            {
                sb.Append($"{ce.Key}:{ce.Value};");
            }

            return sb.ToString();
        }

        private Dictionary<string, decimal> Deserialize(string dic)
        {
            var dictionary = new Dictionary<string, decimal>();
            var kvs = dic.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries);
            foreach (var kv in kvs)
            {
                dictionary.Add(kv.Split(':')[0], decimal.Parse(kv.Split(':')[1]));
            }

            return dictionary;
        }

        private object item;
    }
}