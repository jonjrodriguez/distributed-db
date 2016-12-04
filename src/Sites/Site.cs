using System;
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

        public IList<Variable> Data { get; set; }

        private LockManager LockManager { get; set; }

        public SiteState State { get; set; }

        public int UpSince { get; set; }

        public bool GetReadLock(Transaction transaction, string variableName)
        {
            var variable = GetVariable(variableName);

            if (!variable.Readable)
            {
                return false;
            }

            if (transaction.IsReadOnly)
            {
                return true;
            }

            return LockManager.GetReadLock(transaction, variable);
        }

        public bool GetWriteLock(Transaction transaction, string variableName)
        {
            var variable = GetVariable(variableName);
            
            return LockManager.GetWriteLock(transaction, variable);
        }

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

        public void WriteData(string variableName, int value)
        {
            var variable = GetVariable(variableName);

            variable.NewValue = value;
        }

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

        public void ClearLocks(Transaction transaction)
        {
            LockManager.ClearLocks(transaction);
        }

        public IList<Transaction> GetBlockingTransactions(Transaction transaction, string variable)
        {
            var locks = LockManager.GetBlockingLocks(transaction, variable);
            
            return locks.Select(l => l.Transaction).ToList();
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
        
        public void Fail()
        {
            State = SiteState.Fail;
            LockManager = new LockManager();

            foreach (var variable in Data.Where(v => v.IsReplicated))
            {
                variable.Readable = false;
            }
        }

        public void Recover(int time)
        {
            UpSince = time;
            State = SiteState.Stable;
        }

        public override string ToString()
        {
            return $"Site {Id}";
        }

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
    }
}