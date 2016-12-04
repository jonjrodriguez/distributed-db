using System.Collections.Generic;
using System.Linq;
using DistributedDb.Operations;
using DistributedDb.Sites;

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
            WaitTime = int.MaxValue;
            SitesSeen = new Dictionary<Site, int>();
        }

        public string Name { get; set; }

        public TransactionState State { get; set; }

        public bool IsReadOnly { get; set; }

        public int StartTime { get; set; }

        public int EndTime { get; set; }

        public int WaitTime { get; set; }

        public Operation OperationBuffer { get; set; }

        public IDictionary<Site,int> SitesSeen { get; set; }

        public void AddSite(Site site, int time)
        {
            if (SitesSeen.Any(s => s.Key == site))
            {
                return;
            }
            
            SitesSeen.Add(site, time);
        }

        public IList<Site> GetStableSites()
        {
            return SitesSeen.Where(s => s.Key.State == SiteState.Stable)
                .Select(s => s.Key)
                .ToList();
        }

        public bool CanCommit()
        {
            var allDone = State == TransactionState.Running && OperationBuffer == null;

            var sitesUp = IsReadOnly || SitesSeen.All(s => s.Key.State == SiteState.Stable && s.Key.UpSince <= s.Value);

            return allDone && sitesUp;
        }

        public bool Active()
        {
            return State == TransactionState.Running || IsWaiting();
        }

        public bool IsWaiting()
        {
            return State == TransactionState.Blocked || State == TransactionState.Waiting;
        }

        public void ClearBuffer()
        {
            State = TransactionState.Running;
            OperationBuffer = null;
            WaitTime = int.MaxValue;
        }

        public override string ToString()
        {
            return Name;
        }
    }
}