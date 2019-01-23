using ExpenseTracker.Core;
using ExpenseTracker.GmailConnector;
using ExpenseTracker.UI;
using System.Configuration;
using System.IO;

namespace ExpenseTracker.ConsoleClient
{
    internal class ExtendedMenuBuilder : MenuBuilder
    {
        private readonly ITransactionsService expensesService;
        private readonly ISettings settings;

        public ExtendedMenuBuilder(IMenuFactory factory, IOutputRenderer renderer, ITransactionsService expensesService, ISettings settings) : base(factory, renderer)
        {
            this.expensesService = expensesService;
            this.settings = settings;
        }

        public override MenuBase Build()
        {
            var menu = base.Build();
            if (this.settings.ClientMode != Settings.WebClientModeValue)
            {
                menu.AddAction("ba", () => "backup database", () => BackupFile(this.settings.DbPath));
            }

            menu.AddAction("im", () => "Import gmail", () => this.Import());
            return menu;
        }

        private void BackupFile(string sourcePath)
        {
            var rootPath = Path.GetDirectoryName(sourcePath);
            var baseFileName = Path.GetFileNameWithoutExtension(sourcePath);
            var extension = Path.GetExtension(sourcePath);

            int i = 0;
            var fileName = baseFileName + "_" + i;
            var newPath = rootPath + "\\" + fileName + "." + extension;
            while (File.Exists(newPath))
            {
                i++;
                fileName = baseFileName + "_" + i;
                newPath = rootPath + "\\" + fileName + "." + extension;
            }

            File.Copy(sourcePath, newPath);
        }

        private void Import()
        {
            var user = this.settings.MailUser;
            var pass = this.settings.MailPass;
            new ExpensesGmailImporter(user, pass, this.expensesService).Import();
        }
    }
}