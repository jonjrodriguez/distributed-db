using System.Collections.Generic;

namespace DistributedDb.Locks
{
    public class LockManager
    {
        public IList<Lock> Locks { get; set; }
    }
}