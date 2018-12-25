using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;

namespace ExpenseTracker.ConsoleClient
{
    internal class ItemEditorMenu : MenuBase
    {
        public ItemEditorMenu(object item)
        {
            this.item = item;
            var props = item.GetType().GetProperties();
            var i = 1;
            foreach (var p in props)
            {
                this.AddAction(i.ToString(), p.Name, () => this.Edit(p));
                i++;
            }
        }

        private void Edit(PropertyInfo p)
        {
            var typeObj = this.item.GetType();
            var propValues = this.GetPropVal(p);
            var newValue = Utils.PromptInput("Enter prop value: ", propValues);
            this.SetPropVal(p, newValue, this.item);
        }

        internal string GetPropVal(PropertyInfo p)
        {
            var propertyType = p.PropertyType;
            if (propertyType == typeof(Dictionary<string, decimal>))
            {
                var catExpenditure = (Dictionary<string, decimal>)p.GetValue(item) ?? new Dictionary<string, decimal>();
                return Serialize(catExpenditure);
            }

            return p.GetValue(this.item).ToString();
        }

        private void SetPropVal(PropertyInfo p, string newValue, object obj)
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
                p.SetValue(this.item, Deserialize(newValue));
            }
            else
            {
                throw new ArgumentException("Unknown property type to edit");
            }
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