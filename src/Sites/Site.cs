using System.Collections.Generic;
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
    }
}