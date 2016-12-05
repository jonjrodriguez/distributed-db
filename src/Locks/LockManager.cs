using System.Collections.Generic;
using System.Linq;
using DistributedDb.Transactions;
using DistributedDb.Variables;

namespace DistributedDb.Locks
{
    /// <summary>
    /// Handles locks for a single Site
    /// </summary>
    public class LockManager
    {
        public LockManager()
        {
            Locks = new List<Lock>();
        }

        /// <summary>
        /// List of locks at the Site
        /// </summary>
        private List<Lock> Locks { get; set; }

        /// <summary>
        /// Tries to get a read lock on the given variable for the transaction
        /// If the variable is not readable, we cannot get a lock (return false)
        /// If the transaction is read-only, we do not need a lock (return true)
        /// If the transaction already has a lock, we do not need another (return true)
        /// If there is a write lock, we cannot get a read lock (return false)
        /// Otherwise, create a new read lock
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="variable"></param>
        /// <returns>Whether a read lock was obtained</returns>
        public bool GetReadLock(Transaction transaction, Variable variable)
        {
            if (!variable.Readable)
            {
                return false;
            }

            if (transaction.IsReadOnly)
            {
                return true;
            }

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

        /// <summary>
        /// Tries to get a write/exclusive lock on the given variable for the transaction
        /// If there are any other locks on the variable, we cannot get a lock (return false)
        /// If the transaction already has a lock, we upgrade it to a write lock
        /// Otherwise, we create a new write lock
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="variable"></param>
        /// <returns>Whether a write/exclusive lock can be obtained</returns>
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

        /// <summary>
        /// Returns if the transaction has a write lock on the variable 
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="variable"></param>
        public bool HasWriteLock(Transaction transaction, Variable variable)
        {
            var locks = Locks.Where(l => l.Variable == variable);

            return locks.Any(l => l.Type == LockType.Write && l.Transaction == transaction);
        }

        /// <summary>
        /// Get all the variables writed locked by a certain transaction
        /// </summary>
        /// <param name="transaction"></param>
        /// <returns>List of variables a transaction has written to</returns>
        public IList<Variable> GetWriteLockedData(Transaction transaction)
        {
            var locks = Locks.Where(l => l.Transaction == transaction && l.Type == LockType.Write);

            return locks.Select(l => l.Variable).ToList();
        }

        /// <summary>
        /// Get all the locks that are blocking the given transaction from reading or writing the variable
        /// </summary>
        /// <param name="transaction"></param>
        /// <param name="variableName"></param>
        /// <returns>All locks that are blocking the given transaction from accessing the given variable</returns>
        public IList<Lock> GetBlockingLocks(Transaction transaction, string variableName)
        {
            return Locks
                .Where(l => l.Transaction != transaction && l.Variable.Name == variableName)
                .ToList();
        }

        /// <summary>
        /// Clears all locks for the given transaction
        /// </summary>
        /// <param name="transaction"></param>
        public void ClearLocks(Transaction transaction)
        {
            Locks.RemoveAll(l => l.Transaction == transaction);
        }
    }
}