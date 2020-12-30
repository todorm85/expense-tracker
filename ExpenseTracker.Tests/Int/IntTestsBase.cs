using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.App;
using ExpenseTracker.Core;
using ExpenseTracker.Core.Data;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using Unity;

namespace ExpenseTracker.Tests.Int
{
    public class IntTestsBase
    {
        protected UnityContainer container;
        protected MailClientMock mailClient;
        private const string DirectoryPath = "c:\\temp";
        private static readonly string DbPath = $"{DirectoryPath}\\test.db";

        [TestCleanup]
        public virtual void CleanUp()
        {
            this.container.Resolve<IUnitOfWork>().Dispose();
            this.container.Dispose();
        }

        [TestInitialize]
        public virtual void Initialize()
        {
            InitializeDbFile();
            this.container = new UnityContainer();
            Application.RegisterDependencies(container, GetConfig());
            this.mailClient = new MailClientMock();
            this.container.RegisterInstance<IMailClient>(this.mailClient);
        }

        private static Config GetConfig()
        {
            var config = new Config()
            {
                DbPath = DbPath,
            };

            return config;
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
    }
}