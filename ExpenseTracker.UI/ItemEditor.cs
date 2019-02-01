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
            this.serializer = new Serializer();
        }

        public string GetPropVal(PropertyInfo p)
        {
            return this.serializer.Serialize(p.GetValue(this.item));
        }

        public void SetPropVal(PropertyInfo p, string newValue)
        {
            var propertyType = p.PropertyType;
            p.SetValue(this.item, this.serializer.Deserialize(propertyType, newValue));
        }

        public PropertyInfo[] GetProperties()
        {
            return this.item.GetType().GetProperties();
        }

        private readonly object item;
        private Serializer serializer;
    }
}