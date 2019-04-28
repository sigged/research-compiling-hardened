using System;

namespace Threats.Env.Variables
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine($"Reading Environment variables");
            Console.WriteLine("-------------------------------");
            var envDict = Environment.GetEnvironmentVariables();
            foreach (var env in envDict.Keys)
            {
                Console.WriteLine($"{env} = {Environment.GetEnvironmentVariable(env.ToString())}");
            }
            Console.WriteLine();

            Console.WriteLine($"Changing Environment variable PORT (!)");
            string originalValue = Environment.GetEnvironmentVariable("PORT");
            Environment.SetEnvironmentVariable("PORT", "666");
            Console.WriteLine();

            Console.WriteLine($"Reading Environment variable PORT");
            Console.WriteLine("-----------------------------------");
            Console.WriteLine($"PORT = {Environment.GetEnvironmentVariable("PORT")}");

            Environment.SetEnvironmentVariable("PORT", originalValue);
        }
    }
}
