using System;
using System.Collections.Generic;
using System.Linq;
using DistributedDb.Operations;
using DistributedDb.Sites;
using DistributedDb.Variables;

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
                    case OperationType.Write:
                        WriteVariable(operation.Transaction, operation.Variable, (int) operation.WriteValue);
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

            if (readOnly)
            {
                transaction.LocalData = SiteManager.Snapshot();
            }

            Transactions.Add(transaction);
        }

        public void ReadVariable(string transactionName, string variableName)
        {
            var transaction = GetTransaction(transactionName);

            var variable = transaction.ReadFromLocal(variableName);

            if (variable != null)
            {
                Console.WriteLine($"{transaction.ToString()} reads {variable.ToString()}");
                return;
            }

            var stableSites = SiteManager.SitesWithVariable(variableName, SiteState.Stable);
            foreach (var site in stableSites)
            {
                if (site.GetReadLock(transaction, variableName))
                {
                    variable = site.ReadData(variableName);
                    Console.WriteLine($"{transaction.ToString()} reads {variable.ToString()}");
                    return;
                }
            }
        }

        public void WriteVariable(string transactionName, string variableName, int value)
        {
            var transaction = GetTransaction(transactionName);

            var lockedAllSites = true;
            var stableSites = SiteManager.SitesWithVariable(variableName, SiteState.Stable);
            foreach (var site in stableSites)
            {
                if (!site.GetWriteLock(transaction, variableName))
                {
                    lockedAllSites = false;
                }
            }

            if (lockedAllSites)
            {
                var variable = new Variable { Name = variableName, Value = value };
                transaction.LocalData.Add(variable);
                foreach (var site in stableSites)
                {
                    site.WriteData(variable);
                }
            }
        }

        private Transaction GetTransaction(string transactionName)
        {
            var transaction = Transactions.FirstOrDefault(t => t.Name == transactionName);

            if (transaction == null)
            {
                Console.WriteLine($"Trying to read as transaction {transactionName}, but it doesn't exist.");
                Environment.Exit(1);
            }

            return transaction;
        }
    }
}