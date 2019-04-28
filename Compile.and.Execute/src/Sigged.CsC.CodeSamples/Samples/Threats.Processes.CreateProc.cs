using System;
using System.Diagnostics;
using System.Runtime.InteropServices;

namespace Threats.Processes.CreateProc
{
    class Program
    {
        static void Main(string[] args)
        {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo();

            if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
            {
                process.StartInfo.FileName = @"cmd.exe";
                process.StartInfo.Arguments = @"/C dir";
            }
            else
            {
                process.StartInfo.FileName = @"bash";
                process.StartInfo.Arguments = @"-c ""ls""";
            }

            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardOutput = true;
            process.StartInfo.RedirectStandardInput = true;
            process.StartInfo.RedirectStandardError = true;
            process.OutputDataReceived += Process_OutputDataReceived;
            process.Start();
            process.BeginOutputReadLine();

            System.Threading.Thread.Sleep(1000);
        }

        private static void Process_OutputDataReceived(object sender, DataReceivedEventArgs e)
        {
            Console.WriteLine(e.Data);
        }
    }

}
