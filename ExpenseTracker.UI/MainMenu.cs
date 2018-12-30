using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using ExpenseTracker.Core;
using ExpenseTracker.GmailConnector;

namespace ExpenseTracker.UI
{
    public class MainMenu : MenuBase
    {
        public MainMenu(IExpensesService service, Config config, IOutputRenderer renderer) : base(renderer)
        {
            this.service = service;
            this.config = config;
            this.renderer = renderer;
        }

        [MenuAction("b", "Backup database")]
        public void BackupFile()
        {
            string sourcePath = config.DbPath;
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

        [MenuAction("im", "Import GMail")]
        public void ImportGmail()
        {
            this.GetMailClient().Import();
        }

        [MenuAction("cl", "Classify all expenses")]
        public void Categorize()
        {
            this.service.Classify();
        }

        private ExpensesGmailImporter GetMailClient()
        {
            string user;
            string pass;
            this.GetCredentials(out user, out pass);
            return new ExpensesGmailImporter(user, pass, this.service);
        }

        private void GetCredentials(out string user, out string pass)
        {
            user = config.MailUser;
            if (string.IsNullOrEmpty(user))
            {
                user = Renderer.PromptInput("Mail:");
            }

            pass = config.MailPass;
            if (string.IsNullOrEmpty(pass))
            {
                pass = Renderer.PromptInput("Pass:");
            }
        }

        private IExpensesService service;
        private readonly Config config;
        private readonly IOutputRenderer renderer;
    }
}