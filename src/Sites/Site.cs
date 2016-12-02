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

        public bool GetReadLock(Transaction transaction, string variable)
        {
            return true;
        }

        public bool GetWriteLock(Transaction transaction, string variable)
        {
            return true;
        }

        public Variable ReadData(string variableName)
        {
            var variable = GetVariable(variableName);

            return variable;
        }

        public void WriteData(Variable newVariable)
        {
            var variable = GetVariable(newVariable.Name);

            variable.UpdatedValue = newVariable.Value;
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