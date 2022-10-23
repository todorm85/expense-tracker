using System.Text.Json;

namespace ExpenseTracker.Web.Pages.Shared
{
    public static class ModelSerialization
    {
        public static T Deserialize<T>(string model) where T : class
        {
            if (model == null)
            {
                return null;
            }

            return JsonSerializer.Deserialize<T>(model);
        }

        public static string Serialize(object model)
        {
            if (model == null)
            {
                return null;
            }

            return JsonSerializer.Serialize(model);
        }
    }
}
