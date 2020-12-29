using System.Reflection;

namespace ExpenseTracker.Core.UI
{
    internal class ItemEditor
    {
        private readonly object item;

        private Serializer serializer;

        public ItemEditor(object item)
        {
            this.item = item;
            this.serializer = new Serializer();
        }

        public PropertyInfo[] GetProperties()
        {
            return this.item.GetType().GetProperties();
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
    }
}