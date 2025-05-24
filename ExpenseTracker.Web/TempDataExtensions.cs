using Microsoft.AspNetCore.Mvc.ViewFeatures;
using System.Text.Json;

namespace ExpenseTracker.Web
{
    public static class TempDataExtensions
    {
        public static void Set<T>(this ITempDataDictionary tempData, string key, T value)
        {
            tempData[key] = JsonSerializer.Serialize(value);
        }

        public static T Get<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            tempData.TryGetValue(key, out object value);
            return value == null ? null : JsonSerializer.Deserialize<T>((string)value);
        }

        public static T Peek<T>(this ITempDataDictionary tempData, string key) where T : class
        {
            object value = tempData.Peek(key);
            return value == null ? null : JsonSerializer.Deserialize<T>((string)value);
        }
    }
}
