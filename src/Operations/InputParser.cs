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
            string line;
            if (InputFile != null)
            {
                line = InputFile.ReadLine();
                while (line != null && line.StartsWith("//"))
                {
                    line = InputFile.ReadLine();
                }

                return line;
            } 
            
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
                .Split(new[] { '(', ')' })
                .Where(s => !string.IsNullOrEmpty(s.Trim()))
                .ToArray();

            try
            {
                var type = GetOperationType(parts[0]);

                var info = parts.Length > 1 ? parts[1].Trim()
                    .Split(',')
                    .Where(s => !string.IsNullOrEmpty(s.Trim()))
                    .ToArray() : new string[0];

                if (type != OperationType.Dump && info.Length < 1)
                {
                    throw new System.FormatException("Operation contains no data.");
                }

                var transaction = info.Length > 0 ? GetTransaction(info, type) :  "";
                var site = info.Length > 0 ? GetSite(info, type) : null;
                var variable = info.Length > 0 ? GetVariable(info, type) : "";
                var writeValue = info.Length > 0 ? GetWriteValue(info, type) : null;

                return new Operation
                { 
                    Type = type,
                    Transaction = transaction,
                    Site = site,
                    Variable = variable,
                    WriteValue = writeValue
                };
            }
            catch (System.FormatException e)
            {
                Console.WriteLine($"Operation '{operation}' is invalid. {e.Message}");
                Environment.Exit(1);
                return null;
            }
        }

        private OperationType GetOperationType(string operation)
        {
            switch (operation.Trim().ToLower())
            {
                case "begin":
                    return OperationType.Begin;
                case "beginro":
                    return OperationType.BeginRO;
                case "r":
                    return OperationType.Read;
                case "w":
                    return OperationType.Write;
                case "dump":
                    return OperationType.Dump;
                case "end":
                    return OperationType.End;
                case "fail":
                    return OperationType.Fail;
                case "recover":
                    return OperationType.Recover;
                default:
                    throw new System.FormatException("The operation is not supported");
            }
        }

        public string GetTransaction(string[] info, OperationType type)
        {
            switch (type)
            {
                case OperationType.Begin:
                case OperationType.BeginRO:
                case OperationType.Read:
                case OperationType.Write:
                case OperationType.End:
                    return info[0].Trim();
                case OperationType.Dump:
                case OperationType.Fail:
                case OperationType.Recover:
                default:
                    return "";
            }
        }

        public int? GetSite(string[] info, OperationType type)
        {
            int site;
            if (type == OperationType.Dump)
            {
                if (int.TryParse(info[0], out site))
                {
                    if (site <= 10 && site >= 1)
                    {
                        return site;
                    }

                    throw new System.FormatException("The input needs to be a valid site.");
                }
            }

            if (type == OperationType.Fail || type == OperationType.Recover)
            {
                if (int.TryParse(info[0], out site) && site <= 10 && site >= 1)
                {
                    return site;
                }
                
                throw new System.FormatException("The input needs to be a valid site.");
            }

            return null;
        }

        public string GetVariable(string[] info, OperationType type)
        {
            if (type == OperationType.Dump)
            {
                int site;
                return int.TryParse(info[0], out site) ? "" : GetValidVariable(info[0]); 
            }

            if (type == OperationType.Read || type == OperationType.Write)
            {
                if (info.Length < 2)
                {
                    throw new System.FormatException("Operation doesn't have variable information");
                }

                return GetValidVariable(info[1]);
            }
            
            return "";
        }

        public string GetValidVariable(string variable)
        {
            variable = variable.Trim().ToLower();

            if (!variable.StartsWith("x"))
            {
                throw new System.FormatException("The input needs to be a valid variable.");
            }

            int id;
            if (!int.TryParse(variable.Substring(1), out id) || id > 20 || id < 1)
            {
                throw new System.FormatException("The input needs to be a valid variable.");
            }

            return variable;
        }

        public int? GetWriteValue(string[] info, OperationType type)
        {
            if (type != OperationType.Write)
            {
                return null;
            }

            if (info.Length < 3)
            {
                throw new System.FormatException("Write operation does not contain value to write.");
            }

            return int.Parse(info[2]);
        }
    }
}