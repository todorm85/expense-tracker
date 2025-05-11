using ExpenseTracker.Allianz.Gmail;
using ExpenseTracker.App;
using ExpenseTracker.Core;
using ExpenseTracker.Core.Data;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;

namespace ExpenseTracker.Tests.Int
{
    public class IntTestsBase
    {
        protected IServiceCollection services;
        protected IServiceProvider serviceProvider;
        protected MailClientMock mailClient;
        private const string DirectoryPath = "c:\\temp";
        private static readonly string DbPath = $"{DirectoryPath}\\test.db";

        [TestCleanup]
        public virtual void CleanUp()
        {
            this.serviceProvider.GetService<IUnitOfWork>().Dispose();
        }

        [TestInitialize]
        public virtual void Initialize()
        {
            InitializeDbFile();
            this.services = new ServiceCollection(); // Initialize IServiceCollection
            Application.RegisterDependencies(services, GetConfig());
            this.mailClient = new MailClientMock();
            this.services.AddSingleton<IMailClient>(this.mailClient); // Register MailClientMock as IMailClient
            this.serviceProvider = services.BuildServiceProvider();
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