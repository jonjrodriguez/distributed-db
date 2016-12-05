using DistributedDb.Operations;

namespace DistributedDb.Transactions
{
    public enum TransactionState
    {
        Running,
        Waiting,
        Blocked,
        Committed,
        Aborted
    }

    public class Transaction
    {   
        public Transaction()
        {
            State = TransactionState.Running;
            OperationBuffer = null;
            WaitTime = int.MaxValue;
        }

        public string Name { get; set; }

        /// <summary>
        /// State of the transaction
        /// Running/Waiting/Blocked/Committed/Aborted
        /// </summary>
        public TransactionState State { get; set; }

        /// <summary>
        /// Whether this is a read-only transaction
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Time the transaction started
        /// The lower the time, the older the transaction
        /// </summary>
        public int StartTime { get; set; }

        /// <summary>
        /// Time the transaction was aborted or committed
        /// </summary>
        public int EndTime { get; set; }

        /// <summary>
        /// Time since this transaction has been waiting
        /// </summary>
        public int WaitTime { get; set; }

        /// <summary>
        /// Buffer/Queue of waiting operations for this transaction
        /// Since there can only be one waiting operation, it is a single item
        /// </summary>
        public Operation OperationBuffer { get; set; }

        /// <summary>
        /// Whether a transaction is still Active
        /// Includes Running, Waiting, or Blocked states
        /// Excludes committed or aborted states
        /// </summary>
        public bool Active()
        {
            return State == TransactionState.Running || IsWaiting();
        }

        /// <summary>
        /// Whether the transaction is blocked by another transaction or waiting for an available site
        /// </summary>
        public bool IsWaiting()
        {
            return State == TransactionState.Blocked || State == TransactionState.Waiting;
        }

        /// <summary>
        /// Clears the operation buffer for this transaction
        /// Updates State back to Running
        /// Sets the WaitTime to the max
        /// </summary>
        public void ClearBuffer()
        {
            State = TransactionState.Running;
            OperationBuffer = null;
            WaitTime = int.MaxValue;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}