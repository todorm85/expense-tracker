using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Runtime.Serialization.Json;
using System.Text;

namespace ExpenseTracker.ConsoleClient
{
    internal static class Settings
    {
        static Settings()
        {
            var settingsPath = ConfigurationManager.AppSettings["settingsPath"];
            if (File.Exists(settingsPath))
            {
                var serializer = new DataContractJsonSerializer(typeof(Dictionary<string, string>), new DataContractJsonSerializerSettings()
                {
                    UseSimpleDictionaryFormat = true
                });

                var json = File.OpenText(settingsPath).ReadToEnd();
                var jsonBytes = Encoding.UTF8.GetBytes(json);
                var settingsCollection = serializer.ReadObject(new MemoryStream(jsonBytes)) as Dictionary<string, string>;
                settings = settingsCollection;
            }
        }

        public const string WebClientModeValue = "web";

        private static readonly IDictionary<string, string> settings;

        public static string DbPath { get => GetSetting("DbPath"); }
        public static string ClientMode { get => GetSetting("ClientMode"); }
        public static string WebClientAuth { get => GetSetting("WebClientAuth"); }
        public static string WebServiceBase { get => GetSetting("WebServiceBase"); }

        private static string GetSetting(string key)
        {
            if (settings.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }
            else
            {
                return string.Empty;
            }
        }
    }
}