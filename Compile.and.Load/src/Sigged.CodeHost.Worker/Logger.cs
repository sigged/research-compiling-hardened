using System;
using System.IO;

namespace Sigged.CodeHost.Worker
{
    public static class Logger
    {
        static Logger()
        {
        }

        public static void LogLine(string text)
        {
            var currentOut = Console.Out;
            Console.SetOut(new StreamWriter(Console.OpenStandardOutput()) { AutoFlush = true });

            Console.WriteLine(text);

            Console.SetOut(currentOut);
        }

    }
}
