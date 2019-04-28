using System;

namespace Harmless.TextInput
{
    public class Program
    {
        public static void Main(string[] args)
        {
            Console.Write("What is your name ? ");
            string input = Console.ReadLine();
            Console.WriteLine($"Hello { input },");
            Console.WriteLine($"Nice to meet you");
        }
    }
}