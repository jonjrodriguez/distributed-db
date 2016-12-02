using System;
using System.Collections.Generic;
using System.Linq;
using DistributedDb.Locks;
using DistributedDb.Transactions;
using DistributedDb.Variables;

namespace DistributedDb.Sites
{
    public enum State
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
            State = State.Stable;
        }
        
        public int Id { get; set; }

        public IList<Variable> Data { get; set; }

        public LockManager LockManager { get; set; }

        public State State { get; set; }

        public bool GetReadLock(Transaction transaction, string variable)
        {
            return true;
        }

        public Variable ReadData(string variableName)
        {
            var variable = Data.FirstOrDefault(v => v.Name == variableName);

            if (variable == null)
            {
                Console.WriteLine($"Variable {variableName} doesn't exist at Site {Id}.");
            }

            return variable;
        }

        public string ToString(string variableName)
        {
            var data = string.IsNullOrWhiteSpace(variableName) ? Data : Data.Where(d => d.Name == variableName);
            
            var result = $"Site {Id}:\n";
            foreach (var variable in data)
            {
                result += variable.ToString() + " ";
            }

            return result;
        }
    }
}