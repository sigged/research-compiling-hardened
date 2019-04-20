using System;

namespace Harmless.TextInput
{
    public class Program
    {
        private static Random rnd = new Random();

        public static void Main(string[] args)
        {
            int number = -1;
            bool ok = false;
            do
            {
                Console.WriteLine("");
                Console.Write("Enter a number between 0-9: ");
                char input = (char)Console.Read();
                ok = int.TryParse(input.ToString(), out number);
            }
            while (!ok);

            Console.WriteLine("");
            Console.WriteLine($"{number} random numbers: ");
            for (int i = 0; i < number; i++)
            {
                Console.Write(rnd.Next(1, 100));
                Console.Write(" ");
            }
        }
    }
}