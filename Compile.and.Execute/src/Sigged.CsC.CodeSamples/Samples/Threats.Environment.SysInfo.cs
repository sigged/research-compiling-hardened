using System;

namespace Threats.Env.SysInfo
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Machine = {Environment.MachineName}");
            Console.WriteLine($"OS = {Environment.OSVersion}");
            Console.WriteLine($"System folder = {Environment.SystemDirectory}");
            Console.WriteLine($"Username = {Environment.UserName}");
            Console.WriteLine($"Process = {Environment.CommandLine}");
        }
    }
}
