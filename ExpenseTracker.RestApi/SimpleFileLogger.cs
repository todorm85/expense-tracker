using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ExpenseTracker.RestApi
{
    public class SimpleFileLogger : ICustomLogger
    {
        public SimpleFileLogger(string logPath)
        {
            this.logPath = logPath;
        }

        private string logPath;

        public void Log(string message)
        {
            using (var file = new StreamWriter(logPath, true))
            {
                file.WriteLine($"{DateTime.Now} {message}");
            }
        }
    }
}
