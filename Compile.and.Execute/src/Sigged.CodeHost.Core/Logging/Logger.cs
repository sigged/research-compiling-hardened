using System;
using System.Diagnostics;
using System.IO;

namespace Sigged.CodeHost.Core.Logging
{
    public static class Logger
    {
        static Logger()
        {
            Mode = LogMode.Console;
            try
            {
                string logmode = Environment.GetEnvironmentVariable("CODEHOST_LOGMODE");
                if(logmode?.ToUpper() == "NULL")
                    Mode = LogMode.Null;
                else if (logmode?.ToUpper() == "DEBUG")
                    Mode = LogMode.Debug;
                else
                    Mode = LogMode.Console;
            }
            catch
            {
            }
        }

        public static LogMode Mode { get; set; }

        public static void LogLine(string text)
        {
            switch (Mode)
            {
                case LogMode.Console:
                    var currentOut = Console.Out;
                    Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });
                    Console.WriteLine(text);
                    Console.SetOut(currentOut);
                    break;
                case LogMode.Debug:
                    Debug.WriteLine(text);
                    break;
                default:
                    break;
            }
            
        }

    }
}
