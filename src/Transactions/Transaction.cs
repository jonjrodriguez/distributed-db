using System.Collections.Generic;
using System.Linq;
using DistributedDb.Operations;
using DistributedDb.Variables;

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
            WaitTime = 0;
        }

        public string Name { get; set; }

        public TransactionState State { get; set; }

        public bool IsReadOnly { get; set; }

        public int StartTime { get; set; }

        public Operation OperationBuffer { get; set; }

        public int WaitTime { get; set; }

        public override string ToString()
        {
            return Name;
        }
    }
}