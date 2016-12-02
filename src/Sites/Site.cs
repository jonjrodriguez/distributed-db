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
        Recovering,
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
        }
        
        public int Id { get; set; }

        public IList<Variable> Data { get; set; }

        public LockManager LockManager { get; set; }

        public SiteState State { get; set; }

        public bool GetReadLock(Transaction transaction, string variableName)
        {
            var variable = GetVariable(variableName);

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

            if (LockManager.HasWriteLock(transaction, variable))
            {
                return variable.UpdatedValue;
            }

            return variable.Value;
        }

        public void WriteData(string variableName, int value)
        {
            var variable = GetVariable(variableName);

            variable.UpdatedValue = value;
        }

        private Variable GetVariable(string variableName)
        {
            var variable = Data.FirstOrDefault(v => v.Name == variableName);

            if (variable == null)
            {
                Console.WriteLine($"Variable {variableName} doesn't exist at {ToString()}.");
                Environment.Exit(1);
            }

            return variable;
        }

        public override string ToString()
        {
            return $"Site {Id}";
        }

        public string Dump(string variableName)
        {
            var data = string.IsNullOrWhiteSpace(variableName) ? Data : Data.Where(d => d.Name == variableName);
            
            var result = $"{ToString()}:\n";
            foreach (var variable in data)
            {
                result += variable.ToString() + " ";
            }

            return result;
        }
    }
}