using ExpenseTracker.Core;
using ExpenseTracker.GmailConnector;
using ExpenseTracker.UI;
using System.Configuration;
using System.IO;

namespace ExpenseTracker.ConsoleClient
{
    internal class ExtendedMenuBuilder : MenuBuilder
    {
        private readonly IExpensesService expensesService;

        public ExtendedMenuBuilder(IMenuFactory factory, IOutputRenderer renderer, IExpensesService expensesService) : base(factory, renderer)
        {
            this.expensesService = expensesService;
        }

        public override MenuBase Build()
        {
            var menu = base.Build();
            if (!Program.isWebClientMode)
            {
                menu.AddAction("ba", () => "backup database", () => BackupFile(Utils.GetDbPath()));
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

        private static void GetCredentials(out string user, out string pass)
        {
            var renderer = new Renderer();
            user = ConfigurationManager.AppSettings["mailUser"];
            if (string.IsNullOrEmpty(user))
            {
                user = renderer.PromptInput("Mail:");
            }

            pass = ConfigurationManager.AppSettings["mailKey"];
            if (string.IsNullOrEmpty(pass))
            {
                pass = renderer.PromptInput("Pass:");
            }
        }

        private void Import()
        {
            GetCredentials(out string user, out string pass);
            new ExpensesGmailImporter(user, pass, this.expensesService);
        }
    }
}