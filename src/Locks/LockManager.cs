using System.Collections.Generic;
using System.Linq;
using DistributedDb.Transactions;
using DistributedDb.Variables;

namespace DistributedDb.Locks
{
    public class LockManager
    {
        public LockManager()
        {
            Locks = new List<Lock>();
        }

        public IList<Lock> Locks { get; set; }

        public bool GetReadLock(Transaction transaction, Variable variable)
        {
            var locks = Locks.Where(l => l.Variable == variable);

            if (locks.Any(l => l.Transaction == transaction))
            {
                return true;
            }

            if (locks.Any(l => l.Type == LockType.Write))
            {
                return false;
            }

            Locks.Add(new Lock
            {
                Type = LockType.Read,
                Transaction = transaction,
                Variable = variable
            });

            return true;
        }

        public bool GetWriteLock(Transaction transaction, Variable variable)
        {
            var locks = Locks.Where(l => l.Variable == variable);

            if (locks.Any(l => l.Transaction != transaction))
            {
                return false;
            }

            var tLock = locks.FirstOrDefault(l => l.Transaction == transaction);
            if (tLock != null)
            {
                tLock.Type = LockType.Write;
                return true;
            }

            Locks.Add(new Lock
            {
                Type = LockType.Write,
                Transaction = transaction,
                Variable = variable
            });

            return true;
        }

        public bool HasWriteLock(Transaction transaction, Variable variable)
        {
            var locks = Locks.Where(l => l.Variable == variable);

            return locks.Any(l => l.Type == LockType.Write && l.Transaction == transaction);
        }
    }
}