using System.Collections.Generic;
using System.Linq;
using DistributedDb.Deadlocks;
using DistributedDb.Operations;
using DistributedDb.Sites;

namespace DistributedDb.Transactions
{
    /// <summary>
    /// Handles all operations for transactions
    /// Reruns waiting operations at the beginning of every tick as well as when another transaction has ended
    /// Detects deadlocks when appropriate
    /// </summary>
    public class TransactionManager
    {
        public TransactionManager(Clock clock, SiteManager manager)
        {
            Clock = clock;
            SiteManager = manager;
            Transactions = new List<Transaction>();
            DeadlockManager = new DeadlockManager(manager, clock);
        }

        public Clock Clock { get; set; }

        /// <summary>
        /// All existing transactions
        /// </summary>
        public IList<Transaction> Transactions { get; set; }

        /// <summary>
        /// Holds a list of all sites
        /// Makes functions available to determine which site the transactions should visit
        /// </summary>
        public SiteManager SiteManager { get; set; }

        /// <summary>
        /// Handles deadlock detection and killing
        /// </summary>
        public DeadlockManager DeadlockManager { get; set; }

        /// <summary>
        /// Executes all waiting and new operations
        /// Reruns waiting operations at the beginning of each tick
        /// </summary>
        public void Execute(IEnumerable<Operation> operations)
        {
            RerunTransactions();
            
            foreach (var operation in operations)
            {
                RunOperation(operation);
            }
        }

        /// <summary>
        /// Reruns all operations from transactions that are currently waiting
        /// </summary>
        private void RerunTransactions()
        {
            var transactions = Transactions.Where(t => t.IsWaiting())
                .OrderBy(t => t.WaitTime);

            foreach (var transaction in transactions)
            {
                RunOperation(transaction.OperationBuffer, transaction);
            }
        }

        /// <summary>
        /// Runs the transaction operation
        /// Sends the operation to the correct method
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="transaction">Given if the transaction is being re-run</param>
        public void RunOperation(Operation operation, Transaction transaction = null)
        {
            switch (operation.Type)
            {
                case OperationType.Begin:
                case OperationType.BeginRO:
                    BeginTransaction(operation);
                    break;
                case OperationType.Read:
                    ReadVariable(operation, transaction);
                    break;
                case OperationType.Write:
                    WriteVariable(operation, transaction);
                    break;
                case OperationType.End:
                    EndTransaction(operation);
                    break;
                default:
                    Logger.Fail($"Operation '{operation}' is not supported.");
                    break;
            }
        }

        /// <summary>
        /// Handles the begin/beginRO operations
        /// Creates a new transaction and adds it to the current transactions for the TransactionManager
        /// </summary>
        /// <param name="operation"></param>
        public void BeginTransaction(Operation operation)
        {
            if (Transactions.Any(t => t.Name == operation.Transaction))
            {
                Logger.Fail($"Trying to begin transaction {operation.Transaction} when it already exists.");
            }

            var transaction = new Transaction
            {
                Name = operation.Transaction,
                IsReadOnly = operation.Type == OperationType.BeginRO,
                StartTime = Clock.Time
            };

            Transactions.Add(transaction);
        }

        /// <summary>
        /// Handles the read operation
        /// Finds the available sites where the requested variable is located
        /// Checks each site to see if it can retrieve a read lock
        /// Once a lock is obtained, it reads the variable and clears the buffer
        /// Buffers the operation if no sites are availabe (waiting) or can't retrieve lock (blocked)
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="transaction"></param>
        public void ReadVariable(Operation operation, Transaction transaction = null)
        {
            transaction = transaction ?? GetTransaction(operation.Transaction);
            var variableName = operation.Variable;

            var stableSites = SiteManager.SitesWithVariable(variableName, SiteState.Stable);

            if (stableSites.Count() == 0)
            {
                BufferOperation(transaction, operation, TransactionState.Waiting);
                return;
            }

            foreach (var site in stableSites)
            {
                if (site.GetReadLock(transaction, variableName))
                {
                    var value = site.ReadData(transaction, variableName);
                    transaction.AddSite(site, Clock.Time);
                    Logger.Write($"{Clock.ToString()} Transaction {transaction.ToString()} reads {variableName}={value} from {site.ToString()}.");
                    transaction.ClearBuffer();
                    return;
                }
            }

            BufferOperation(transaction, operation, TransactionState.Blocked);
        }

        /// <summary>
        /// Handles the write operation
        /// Finds the available sites where the requested variable is located
        /// Checks each site to see if it can retrieve a write lock
        /// If all locks obtained, it writes the variable at each site and clears the buffer
        /// Buffers the operation if no sites are availabe (waiting) or can't retrieve all locks (blocked)
        /// </summary>
        /// <param name="operation"></param>
        /// <param name="transaction"></param>
        public void WriteVariable(Operation operation, Transaction transaction = null)
        {
            transaction = transaction ?? GetTransaction(operation.Transaction);
            var variableName = operation.Variable;
            var newValue = (int) operation.WriteValue;

            var lockedAllSites = true;
            var stableSites = SiteManager.SitesWithVariable(variableName, SiteState.Stable);

            if (stableSites.Count() == 0)
            {
                BufferOperation(transaction, operation, TransactionState.Waiting);
                return;
            }

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
                transaction.ClearBuffer();
                foreach (var site in stableSites)
                {
                    site.WriteData(variableName, newValue);
                }
            }
            else
            {
                BufferOperation(transaction, operation, TransactionState.Blocked);
            }
        }

        /// <summary>
        /// Handles the end operation
        /// Either commits or aborts the transaction
        /// Sets the end time of the transaction
        /// Reruns any waiting operations
        /// </summary>
        /// <param name="operation"></param>
        public void EndTransaction(Operation operation)
        {
            var transaction = GetTransaction(operation.Transaction);

            transaction.EndTime = Clock.Time;
            if (transaction.CanCommit())
            {
                Commit(transaction);
            }
            else
            {
                Abort(transaction);
            }

            RerunTransactions();
        }

        /// <summary>
        /// Commits the transaction
        /// Marks it as committed and clears all locks at available sites
        /// </summary>
        /// <param name="transaction"></param>
        public void Commit(Transaction transaction)
        {
            Logger.Write($"{Clock.ToString()} Transaction {transaction.ToString()} committed.");
            transaction.State = TransactionState.Committed;
            foreach (var site in transaction.GetStableSites())
            {
                site.CommitValue(transaction);
                site.ClearLocks(transaction);
            }
        }

        /// <summary>
        /// Aborts the transaction
        /// Marks it as aborted and clears all locks at available sites
        /// </summary>
        /// <param name="transaction"></param>
        public void Abort(Transaction transaction)
        {
            Logger.Write($"{Clock.ToString()} Transaction {transaction.ToString()} aborted.");
            transaction.State = TransactionState.Aborted;
            foreach (var site in transaction.GetStableSites())
            {
                site.ClearLocks(transaction);
            }
        }

        /// <summary>
        /// Buffers the given operation on the transaction
        /// Sets the waiting time and whether it is waiting or blocked
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="operation"></param>
        /// <param name="state"></param>
        private void BufferOperation(Transaction transaction, Operation operation, TransactionState state)
        {
            if (transaction.OperationBuffer == null) 
            {
                transaction.State = state;
                transaction.WaitTime = Clock.Time;
                transaction.OperationBuffer = operation;
            }
        }

        /// <summary>
        /// Retrieves the transaction for the current operation
        /// If the transaction was waiting on a previous operation, we run deadlock detection and rerun any waiting transactions
        /// </summary>
        /// <param name="transactionName">name of the transaction for the current operation</param>
        /// <returns></returns>
        private Transaction GetTransaction(string transactionName)
        {
            var transaction = Transactions.FirstOrDefault(t => t.Name.ToLower() == transactionName.ToLower());

            if (transaction == null)
            {
                Logger.Fail($"Trying to read as transaction {transactionName}, but it doesn't exist.");
            }

            if (transaction.IsWaiting())
            {
                DeadlockManager.DetectDeadlocks(Transactions);
                RerunTransactions();
            }

            if (transaction.State != TransactionState.Running)
            {   
                Logger.Fail($"Transaction {transactionName} received another operation while it is {transaction.State}.");
            }

            return transaction;
        }
    }
}