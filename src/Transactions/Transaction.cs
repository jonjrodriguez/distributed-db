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

        /// <summary>
        /// State of the transaction
        /// Running/Waiting/Blocked/Committed/Aborted
        /// </summary>
        public TransactionState State { get; set; }

        /// <summary>
        /// Whether this is a read-only transaction
        /// </summary>
        public bool IsReadOnly { get; set; }

        /// <summary>
        /// Time the transaction started
        /// The lower the time, the older the transaction
        /// </summary>
        public int StartTime { get; set; }

        /// <summary>
        /// Time the transaction was aborted or committed
        /// </summary>
        public int EndTime { get; set; }

        /// <summary>
        /// Time since this transaction has been waiting
        /// </summary>
        public int WaitTime { get; set; }

        /// <summary>
        /// Buffer/Queue of waiting operations for this transaction
        /// Since there can only be one waiting operation, it is a single item
        /// </summary>
        public Operation OperationBuffer { get; set; }

        /// <summary>
        /// List of sites this transaction has had any action with
        /// Includes locks, read, and writes
        /// </summary>
        public IDictionary<Site,int> SitesSeen { get; set; }

        /// <summary>
        /// Adds a site to the list of sites this transaction has visited along with the time
        /// Will add the site on the first visit only
        /// </summary>
        /// <param name="site">The site visited</param>
        /// <param name="time">The time of the visit</param>
        public void AddSite(Site site, int time)
        {
            if (SitesSeen.Any(s => s.Key == site))
            {
                return;
            }
            
            SitesSeen.Add(site, time);
        }

        /// <summary>
        /// Gets the list of sites this transaction has visited which are currently available
        /// </summary>
        /// <returns>List of all stable (up) sites</returns>
        public IList<Site> GetStableSites()
        {
            return SitesSeen.Where(s => s.Key.State == SiteState.Stable)
                .Select(s => s.Key)
                .ToList();
        }

        /// <summary>
        /// Determines whether this transaction can commit
        /// Makes sure there are no operations waiting in the buffer
        /// Checks if transaction is readonly or all available sites the transaction has visited has been up since the transaction first visited
        /// </summary>
        public bool CanCommit()
        {
            var allDone = State == TransactionState.Running && OperationBuffer == null;

            var sitesUp = IsReadOnly || SitesSeen.All(s => s.Key.State == SiteState.Stable && s.Key.UpSince <= s.Value);

            return allDone && sitesUp;
        }

        /// <summary>
        /// Whether a transaction is still Active
        /// Includes Running, Waiting, or Blocked states
        /// Excludes committed or aborted states
        /// </summary>
        public bool Active()
        {
            return State == TransactionState.Running || IsWaiting();
        }

        /// <summary>
        /// Whether the transaction is blocked by another transaction or waiting for an available site
        /// </summary>
        public bool IsWaiting()
        {
            return State == TransactionState.Blocked || State == TransactionState.Waiting;
        }

        /// <summary>
        /// Clears the operation buffer for this transaction
        /// Updates State back to Running
        /// Sets the WaitTime to the max
        /// </summary>
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