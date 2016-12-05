using System;

namespace DistributedDb
{
    /// <summary>
    /// Helper class to write to console
    /// </summary>
    public static class Logger
    {
        /// <summary>
        /// Writes to console in red and exits
        /// </summary>
        /// <param name="message">Message to write to console</param>
        public static void Fail(string message)
        {
            var orgCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Red;

            Console.WriteLine(message);

            Console.ForegroundColor = orgCol;
            Environment.Exit(1);
        }

        /// <summary>
        /// Writes to console in green
        /// </summary>
        /// <param name="message">Message to write to console</param>
        public static void Success(string message)
        {
            var orgCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Green;

            Console.WriteLine(message);

            Console.ForegroundColor = orgCol;
        }

        /// <summary>
        /// Writes to console in yellow
        /// </summary>
        /// <param name="message">Message to write to console</param>
        public static void Info(string message)
        {
            var orgCol = Console.ForegroundColor;
            Console.ForegroundColor = ConsoleColor.Yellow;

            Console.WriteLine(message);

            Console.ForegroundColor = orgCol;
        }

        /// <summary>
        /// Writes to console in default color
        /// </summary>
        /// <param name="message">Message to write to console</param>
        public static void Write(string message = "")
        {
            Console.WriteLine(message);
        }
    }
}