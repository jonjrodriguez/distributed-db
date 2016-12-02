using System.Collections.Generic;
using System.Linq;
using DistributedDb.Locks;
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

        public string ToString(string variable)
        {
            var data = string.IsNullOrWhiteSpace(variable) ? Data : Data.Where(d => d.Name == variable);
            
            var result = $"Site {Id}:\n";
            foreach (var datum in data)
            {
                result += $"{datum.Name}:{datum.Value} ";
            }

            return result;
        }
    }
}