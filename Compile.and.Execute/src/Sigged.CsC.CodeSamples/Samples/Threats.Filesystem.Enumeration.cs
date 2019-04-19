using System;
using System.IO;

namespace Threats.Filesystem.Enumeration
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("Files in current directory...");
            Console.WriteLine("-----------------------------");
            //var path = Environment.CurrentDirectory;
            var path = Path.GetDirectoryName(new Uri(typeof(Program).Assembly.GetName().CodeBase).LocalPath);
            ListFilesAndFolders(path);
            Console.Write("");
            Console.WriteLine("Files in root directory...");
            Console.WriteLine("-----------------------------");
            var root = Path.GetPathRoot(path);
            ListFilesAndFolders(root);

        }

        static void ListFilesAndFolders(string folderPath)
        {
            DirectoryInfo directory = new DirectoryInfo(folderPath);
            foreach (var fso in directory.EnumerateFileSystemInfos())
            {
                Console.WriteLine(fso.Name);
            }
        }
    }
}
