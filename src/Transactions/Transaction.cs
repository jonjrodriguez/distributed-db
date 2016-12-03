using System.Collections.Generic;
using System.Linq;
using DistributedDb.Operations;
using DistributedDb.Sites;
using DistributedDb.Variables;

namespace DistributedDb.Transactions
{
    public enum TransactionState
    {
        Running,
        Waiting,
        Blocked,
        Committed,
        Aborted
    }

    public class Transaction
    {   
        public Transaction()
        {
            State = TransactionState.Running;
            OperationBuffer = null;
            WaitTime = 0;
            SitesSeen = new List<Site>();
        }

        public string Name { get; set; }

        public TransactionState State { get; set; }

        public bool IsReadOnly { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }

        public int WaitTime { get; set; }

        public Operation OperationBuffer { get; set; }

        public IList<Site> SitesSeen { get; set; }

        public void AddSite(Site site)
        {
            if (SitesSeen.Any(s => s == site))
            {
                return;
            }
            
            SitesSeen.Add(site);
        }

        public bool CanCommit()
        {
            var allDone = State == TransactionState.Running && OperationBuffer == null;

            var sitesUp = IsReadOnly || SitesSeen.All(s => s.State == SiteState.Stable && s.UpSince < StartTime);

            return allDone && sitesUp;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}