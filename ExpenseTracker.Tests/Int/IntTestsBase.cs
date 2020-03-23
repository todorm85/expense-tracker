using System.IO;
using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.App;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Unity;

namespace ExpenseTracker.Tests.Int
{
    public class IntTestsBase
    {
        private const string DirectoryPath = "c:\\temp";
        private static readonly string DbPath = $"{DirectoryPath}\\test.db";
        protected UnityContainer container;

        [TestInitialize]
        public void Initialize()
        {
            InitializeDbFile();
            this.container = new UnityContainer();
            Application.RegisterDependencies(container, GetConfig());
            this.container.RegisterType<IMailClient, MailClientMock>();
        }

        private void InitializeDbFile()
        {
            if (!Directory.Exists(DirectoryPath))
            {
                Directory.CreateDirectory(DirectoryPath);
            }

            if (File.Exists(DbPath))
            {
                File.Delete(DbPath);
            }
        }

        private static IConfig GetConfig()
        {
            var config = new Config()
            {
                DbPath = DbPath,
            };

            return config;
        }
    }
}
