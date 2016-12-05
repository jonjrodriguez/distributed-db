using System.Collections.Generic;
using System.Linq;
using DistributedDb.Locks;
using DistributedDb.Transactions;
using DistributedDb.Variables;

namespace DistributedDb.Sites
{
    public enum SiteState
    {
        Stable,
        Fail
    }

    public class Site
    {
        public Site(int id, IList<Variable> data)
        {
            Id = id;
            Data = data;
            LockManager = new LockManager();
            State = SiteState.Stable;
            UpSince = 0;
        }
        
        public int Id { get; set; }

        /// <summary>
        /// List of variables stored at this site
        /// </summary>
        public IList<Variable> Data { get; set; }

        /// <summary>
        /// LockManager responsible for handling locks at this site
        /// </summary>
        private LockManager LockManager { get; set; }

        /// <summary>
        /// Current state of the site
        /// Stable (available/up) or Fail
        /// </summary>
        public SiteState State { get; set; }

        /// <summary>
        /// Time since site last recovered
        /// </summary>
        public int UpSince { get; set; }

        /// <summary>
        /// Attempts to get a read lock for the transaction and variable
        /// </summary>
        /// <param name="transaction">Transaction reading the variable</param>
        /// <param name="variableName">Variable the transaction is attempting to read</param>
        /// <returns>Whether a read lock was attained</returns>
        public bool GetReadLock(Transaction transaction, string variableName)
        {
            var variable = GetVariable(variableName);

            return LockManager.GetReadLock(transaction, variable);
        }

        /// <summary>
        /// Attempts to get a write/exclusive lock for the transaction and variable
        /// </summary>
        /// <param name="transaction">Transaction writing to the variable</param>
        /// <param name="variableName">Variable the transaction is attempting to write</param>
        /// <returns>Whether a write lock was attained</returns>
        public bool GetWriteLock(Transaction transaction, string variableName)
        {
            var variable = GetVariable(variableName);
            
            return LockManager.GetWriteLock(transaction, variable);
        }

        /// <summary>
        /// Gets the value of the variable
        /// If the transaction is read-only, it reads the value at the time the transaction started
        /// If the transaction has a write lock, it reads the value it has written
        /// Otherwise, it reads the latest committed value
        /// </summary>
        /// <param name="transaction">Transaction reading the variable</param>
        /// <param name="variableName">Variable to be read</param>
        /// <returns>Value of the variable</returns>
        public int ReadData(Transaction transaction, string variableName)
        {
            var variable = GetVariable(variableName);

            if (transaction.IsReadOnly)
            {
                return variable.ValueAtTime(transaction.StartTime);
            }

            if (LockManager.HasWriteLock(transaction, variable))
            {
                return variable.NewValue;
            }

            return variable.LatestValue();
        }

        /// <summary>
        /// Writes value to temporary location in variable
        /// </summary>
        /// <param name="variableName">Variable to write to</param>
        /// <param name="value">Value to write to variable</param>
        public void WriteData(string variableName, int value)
        {
            var variable = GetVariable(variableName);

            variable.NewValue = value;
        }

        /// <summary>
        /// Commits the written values to History at the time this transaction has committed
        /// Commits all variables that the given transaction has written to
        /// Sets the variable to readable in case this site was recovering
        /// </summary>
        /// <param name="transaction">Transaction that has committed</param>
        public void CommitValue(Transaction transaction)
        {
            var variables = LockManager.GetWriteLockedData(transaction);

            foreach (var variable in variables)
            {
                variable.History.Add(transaction.EndTime, variable.NewValue);
                variable.NewValue = 0;
                variable.Readable = true;
            }
        }

        /// <summary>
        /// Clear all locks for the given transaction
        /// </summary>
        /// <param name="transaction">Transaction that is ending</param>
        public void ClearLocks(Transaction transaction)
        {
            LockManager.ClearLocks(transaction);
        }

        /// <summary>
        /// Finds all transactions that are blocking the access of the given transaction to the given variable
        /// </summary>
        /// <param name="transaction">Transaction trying to read/write</param>
        /// <param name="variable">Variable transaction is trying to read/write</param>
        /// <returns>All transactions that are blocking the given transaction</returns>
        public IList<Transaction> GetBlockingTransactions(Transaction transaction, string variable)
        {
            var locks = LockManager.GetBlockingLocks(transaction, variable);
            
            return locks.Select(l => l.Transaction).ToList();
        }

        /// <summary>
        /// Fail the current site
        /// Marks the site as failed
        /// Clears all locks at the current site
        /// Sets all replicated variable as not readable
        /// </summary>
        public void Fail()
        {
            State = SiteState.Fail;
            LockManager = new LockManager();

            foreach (var variable in Data.Where(v => v.IsReplicated))
            {
                variable.Readable = false;
            }
        }

        /// <summary>
        /// Recovers a site
        /// Sets it as stable and updates the time is has been up since
        /// </summary>
        /// <param name="time">The time this site recovered</param>
        public void Recover(int time)
        {
            UpSince = time;
            State = SiteState.Stable;
        }

        /// <summary>
        /// Dumps out the current data at the site
        /// If a variable passed, only that variable is dumped out
        /// </summary>
        /// <param name="variableName">Optional variable to dump</param>
        public string Dump(string variableName)
        {
            var data = string.IsNullOrWhiteSpace(variableName) ? Data : Data.Where(d => d.Name == variableName);
            
            var result = $"\t{ToString()} ({State}): ";
            foreach (var variable in data)
            {
                result += variable.ToString() + " ";
            }

            return result;
        }

        private Variable GetVariable(string variableName)
        {
            var variable = Data.FirstOrDefault(v => v.Name == variableName);

            if (variable == null)
            {
                Logger.Fail($"Variable {variableName} doesn't exist at {ToString()}.");
            }

            return variable;
        }
        
        public override string ToString()
        {
            return $"Site {Id}";
        }
    }
}