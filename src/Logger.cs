using System;

namespace DistributedDb
{
    public static class Logger
    {
        public static void Fail(string message)
        {
            var orgCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(message);

            Console.ForegroundColor = orgCol;
            Environment.Exit(1);
        }

        public static void Success(string message)
        {
            var orgCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(message);

            Console.ForegroundColor = orgCol;
        }

        public static void Info(string message)
        {
            var orgCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(message);

            Console.ForegroundColor = orgCol;
        }

        public static void Write(string message = "")
        {
            Console.WriteLine(message);
        }
    }
}