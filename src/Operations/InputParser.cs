using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace DistributedDb.Operations
{
    public class InputParser
    {
        public InputParser(string[] args)
        {
            if (args.Length > 0) 
            {
                if (File.Exists(args[0]))
                {
                    InputFile = File.OpenText(args[0]);
                }
                else 
                {
                    Console.WriteLine($"File not found: {args[0]}");
                    Environment.Exit(1);
                }
            }

            if (InputFile == null)
            {
                Console.WriteLine("Reading from standard input");
                Console.WriteLine("(type exit to quit)");
                Console.WriteLine();
            }
        }

        public StreamReader InputFile { get; set; }

        public List<Operation> GetInstruction()
        {
            var line = GetLine();

            return line != null ? ParseLine(line) : null;
        }

        private string GetLine()
        {
            if (InputFile != null)
            {
                return InputFile.ReadLine();
            } 
            
            string line;
            line = Console.ReadLine();

            return line != "exit" ? line : null;
        }

        private List<Operation> ParseLine(string line)
        {
            var operations = line.Split(';')
                .Where(s => !string.IsNullOrEmpty(s.Trim()));

            var parsedOperations = new List<Operation>();
            foreach (var operation in operations)
            {
                parsedOperations.Add(ParseOperation(operation));
            }

            return parsedOperations;
        }

        private Operation ParseOperation(string operation)
        {
            return new Operation();
        }
    }
}