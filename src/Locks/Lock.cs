using DistributedDb.Transactions;
using DistributedDb.Variables;

namespace DistributedDb.Locks
{
    public enum LockType
    {
        Read,
        Write    
    }

    public class Lock
    {
        public LockType Type { get; set; }

        public Transaction Transaction { get; set; }

        public Variable Variable { get; set; }
    }
}