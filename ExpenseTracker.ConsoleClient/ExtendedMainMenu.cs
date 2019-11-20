using System;
using System.IO;
using ExpenseTracker.AllianzTxtParser;
using ExpenseTracker.Core;
using ExpenseTracker.Core.UI;
using ExpenseTracker.UI;

namespace ExpenseTracker.ConsoleClient
{
    internal class ExtendedMainMenu : MainMenu
    {
        private readonly ITransactionsService transactionsService;

        public ExtendedMainMenu(IOutputProvider output, IInputProvider input, ITransactionsService transactionsService) : base(output, input)
        {
            this.transactionsService = transactionsService;
            if (Settings.ClientMode != Settings.WebClientModeValue)
            {
                this.AddAction("ba", () => "backup database", () => this.BackupFile(Settings.DbPath));
            }

            this.AddAction("im", () => "Import text", () => this.Import());
        }

        public void BackupFile(string sourcePath)
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

        public void Import()
        {
            Console.Write("Provide path to file: ");
            var filePath = Console.ReadLine();

            var parser = new TxtFileParser();
            var ts = parser.ParseFromFile(filePath);

            this.transactionsService.Add(ts);
        }
    }
}
