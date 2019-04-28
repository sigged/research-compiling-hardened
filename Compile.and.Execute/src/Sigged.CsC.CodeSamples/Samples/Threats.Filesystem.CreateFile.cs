using System;
using System.IO;

namespace Threats.Filesystem.CreateFile
{
    class Program
    {
        static string execPath;
        static string rootPath;

        static Program()
        {
            execPath = Environment.CurrentDirectory;
            //execPath = Path.GetDirectoryName(new Uri(typeof(Program).Assembly.GetName().CodeBase).LocalPath);
            rootPath = Path.GetPathRoot(execPath);
        }

        static void Main(string[] args)
        {
            var filePath = Path.Combine(rootPath, "remoteuserfile.threat");

            Console.WriteLine($"Creating {filePath}");
            Console.WriteLine("-------------------------------");
            CreateText(filePath, "This is created by a remote user\n");
            AppendText(filePath, "Quite a threat.\n");
            Console.WriteLine();
            Console.WriteLine($"Reading {filePath}");
            Console.WriteLine("-------------------------------");
            Console.Write(File.ReadAllText(filePath));
            Console.WriteLine();
            Console.WriteLine($"Deleting {filePath}");
            Console.WriteLine("-------------------------------");
            File.Delete(filePath);
        }

        static void AppendText(string filePath, string contents)
        {
            using (var sw = File.AppendText(filePath)) { sw.Write(contents); }
        }

        static void CreateText(string filePath, string contents)
        {
            using (var sw = File.AppendText(filePath)) { sw.Write(contents); }
        }
    }
}
