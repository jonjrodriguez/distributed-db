using DistributedDb.Transactions;
using DistributedDb.Variables;

namespace DistributedDb.Locks
{
    public enum LockType
    {
        Read,
        Write    
    }

    /// <summary>
    /// Representation of a lock
    /// Can be a read or write lock
    /// Each lock has a transaction and variable
    /// </summary>
    public class Lock
    {
        public LockType Type { get; set; }

        public Transaction Transaction { get; set; }

        public Variable Variable { get; set; }
    }
}