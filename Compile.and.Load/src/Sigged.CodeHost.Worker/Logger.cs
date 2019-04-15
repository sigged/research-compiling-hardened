using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Sigged.CodeHost.Worker
{
    public static class Logger
    {
        static string logPath = null;

        static Logger()
        {
            logPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "logfile.txt");
            if (File.Exists(logPath))
            {
                File.Delete(logPath);
            }
            File.CreateText(logPath).Close();
        }

        public static void AppendLogFile(string text)
        {
            using (StreamWriter sw = new StreamWriter(File.Open(logPath, FileMode.Append, FileAccess.Write, FileShare.Read)))
            {
                sw.WriteLine(text);
                sw.Flush();
            }
        }
    }
}
