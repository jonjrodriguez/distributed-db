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
            var parts = operation.Trim()
                .Split(new char[]{ '(', ')' })
                .Where(s => !string.IsNullOrEmpty(s.Trim()))
                .ToArray();

            var type = GetOperationType(parts[0]);

            return new Operation { Type = type };
        }

        private OperationType GetOperationType(string operation)
        {
            OperationType type;
            switch (operation.Trim().ToLower())
            {
                case "begin":
                    type = OperationType.Begin;
                    break;
                case "beginro":
                    type = OperationType.BeginRO;
                    break;
                case "r":
                    type = OperationType.Read;
                    break;
                case "w":
                    type = OperationType.Write;
                    break;
                case "dump":
                    type = OperationType.Dump;
                    break;
                case "end":
                    type = OperationType.End;
                    break;
                case "fail":
                    type = OperationType.Fail;
                    break;
                case "recover":
                    type = OperationType.Recover;
                    break;
                default:
                    type = OperationType.Invalid;
                    break;
            }

            if (type == OperationType.Invalid)
            {
                Console.WriteLine($"Operation '{operation}' is invalid.");
                Environment.Exit(1);
            }

            return type;
        }
    }
}