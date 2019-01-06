using System.Collections.Generic;

namespace ExpenseTracker.ConsoleClient
{
    internal class Settings : ISettings
    {
        public const string WebClientModeValue = "web";

        private readonly IDictionary<string, string> settings;

        public Settings(IDictionary<string, string> settings)
        {
            this.settings = settings;
        }

        public string DbPath { get => this.GetSetting("DbPath"); }
        public string MailUser { get => this.GetSetting("MailUser"); }
        public string MailPass { get => this.GetSetting("MailPass"); }
        public string ClientMode { get => this.GetSetting("ClientMode"); }
        public string WebClientAuth { get => this.GetSetting("WebClientAuth"); }
        public string WebServiceBase { get => this.GetSetting("WebServiceBase"); }

        private string GetSetting(string key)
        {
            if (this.settings.TryGetValue(key, out var value) && !string.IsNullOrEmpty(value))
            {
                return value;
            }
            else
            {
                return new Renderer().PromptInput($"Enter value for {key}");
            }
        }
    }
}