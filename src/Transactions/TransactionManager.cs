using System;
using System.Collections.Generic;
using System.Linq;
using DistributedDb.Operations;
using DistributedDb.Sites;

namespace DistributedDb.Transactions
{
    public class TransactionManager
    {
        public TransactionManager(SiteManager manager)
        {
            Transactions = new List<Transaction>();
            SiteManager = manager;
        }

        public IList<Transaction> Transactions { get; set; }

        public SiteManager SiteManager { get; set; }

        public void Execute(IEnumerable<Operation> operations)
        {
            foreach (var operation in operations)
            {
                switch (operation.Type)
                {
                    case OperationType.Begin:
                        BeginTransaction(operation.Transaction);
                        break;
                    case OperationType.BeginRO:
                        BeginTransaction(operation.Transaction, true);
                        break;
                    case OperationType.Read:
                        ReadVariable(operation.Transaction, operation.Variable);
                        break;
                    default:
                        Console.WriteLine(operation.ToString());
                        break;
                }
            }
        }

        public void BeginTransaction(string transactionName, bool readOnly = false)
        {
            if (Transactions.Any(t => t.Name == transactionName))
            {
                Console.WriteLine($"Trying to begin transaction {transactionName} when it already exists.");
                Environment.Exit(1);
            }

            var transaction = new Transaction
            {
                Name = transactionName,
                IsReadOnly = readOnly
            };

            Transactions.Add(transaction);
        }

        public void ReadVariable(string transactionName, string variableName)
        {
            var transaction = Transactions.FirstOrDefault(t => t.Name == transactionName);

            if (transaction == null)
            {
                Console.WriteLine($"Trying to read as transaction {transactionName}, but it doesn't exist.");
                Environment.Exit(1);
            }

            var sites = SiteManager.SitesWithVariable(variableName);
            foreach (var site in sites)
            {
                if (site.GetReadLock(transaction, variableName))
                {
                    var variable = site.ReadData(variableName);
                    Console.WriteLine($"{transactionName} reads " + variable.ToString() + $" from Site {site.Id}");
                    return;
                }
            }

            Console.WriteLine(transaction.ToString());
        }
    }
}