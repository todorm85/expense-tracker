using System;
using System.IO;

namespace ExpenseTracker.ConsoleClient
{
    public class Utils
    {
        public static void BackupFile(string sourcePath)
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

        public static string Prompt(string msg, string defaultValue = "")
        {
            Console.WriteLine(msg);
            System.Windows.Forms.SendKeys.SendWait(defaultValue);
            return Console.ReadLine();
        }
    }
}