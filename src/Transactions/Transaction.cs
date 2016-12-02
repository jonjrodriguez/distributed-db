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
            SnapShot = new List<Variable>();
        }

        public string Name { get; set; }

        public TransactionState State { get; set; }

        public bool IsReadOnly { get; set; }

        public Operation OperationBuffer { get; set; }

        public IList<Variable> SnapShot { get; set; }

        public Variable ReadFromSnapShot(string variableName)
        {
            return SnapShot.FirstOrDefault(v => v.Name == variableName);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}