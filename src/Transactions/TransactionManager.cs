using System;
using System.Collections.Generic;
using System.Linq;
using DistributedDb.Operations;
using DistributedDb.Sites;

namespace DistributedDb.Transactions
{
    public class TransactionManager
    {
        public TransactionManager(Clock clock, SiteManager manager)
        {
            Clock = clock;
            Transactions = new List<Transaction>();
            SiteManager = manager;
        }

        public Clock Clock { get; set; }

        public IList<Transaction> Transactions { get; set; }

        public SiteManager SiteManager { get; set; }

        public void Execute(IEnumerable<Operation> operations)
        {
            foreach (var operation in operations)
            {
                RunOperation(operation);
            }
        }

        public void RunOperation(Operation operation)
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
                case OperationType.End:
                    EndTransaction(operation.Transaction);
                    break;
                default:
                    Console.WriteLine($"Operation '{operation}' is is not supported.");
                    Environment.Exit(1);
                    break;
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
                IsReadOnly = readOnly,
                StartTime = Clock.Time
            };

            Transactions.Add(transaction);
        }

        public void ReadVariable(string transactionName, string variableName)
        {
            var transaction = GetTransaction(transactionName);

            var stableSites = SiteManager.SitesWithVariable(variableName, SiteState.Stable);
            foreach (var site in stableSites)
            {
                if (transaction.IsReadOnly || site.GetReadLock(transaction, variableName))
                {
                    var variable = site.ReadData(transaction, variableName);
                    transaction.AddSite(site, Clock.Time);
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
                else
                {
                    transaction.AddSite(site, Clock.Time);
                }
            }

            if (lockedAllSites)
            {
                foreach (var site in stableSites)
                {
                    site.WriteData(variableName, value);
                }

                return;
            }
        }

        public void EndTransaction(string transactionName)
        {
            var transaction = GetTransaction(transactionName);

            transaction.EndTime = Clock.Time;
            if (transaction.CanCommit())
            {
                Commit(transaction);
            }
            else
            {
                Abort(transaction);
            }
        }

        public void Commit(Transaction transaction)
        {
            Console.WriteLine("Commit " + transaction.ToString());
            foreach (var site in transaction.GetStableSites())
            {
                site.CommitValue(transaction);
                site.ClearLocks(transaction);
            }
        }

        public void Abort(Transaction transaction)
        {
            Console.WriteLine("Abort " + transaction.ToString());
            foreach (var site in transaction.GetStableSites())
            {
                site.ClearLocks(transaction);
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

            if (transaction.OperationBuffer != null)
            {
                Console.WriteLine($"Transaction {transactionName} received another operation while it is {transaction.State}.");
                Environment.Exit(1);
            }

            return transaction;
        }
    }
}