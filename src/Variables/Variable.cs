using System.Collections.Generic;
using System.Linq;

namespace DistributedDb.Variables
{
    /// <summary>
    /// Representation of a piece of data
    /// Has commit history and temporary write value
    /// </summary>
    public class Variable
    {
        public Variable(int id, int time)
        {
            Id = id;
            Name = "x" + id;
            IsReplicated = id % 2 == 0;
            Readable = true;
            History = new Dictionary<int, int>();
            History.Add(time, id * 10);
        }

        public int Id { get; set; }

        public string Name { get; set; }

        /// <summary>
        /// History of values based on time committed
        /// </summary>
        public Dictionary<int, int> History { get; set; }

        /// <summary>
        /// Value that has been written, but not yet committed
        /// </summary>
        public int NewValue { get; set; }

        /// <summary>
        /// If this variable is stored at multiple sites
        /// </summary>
        public bool IsReplicated { get; set; }

        /// <summary>
        /// If this variable is Readable
        /// True by default or if a variable is unreplicated
        /// Set to false when the site this variable is stored at fails
        /// </summary>
        public bool Readable { get; set; }

        /// <summary>
        /// Finds the latest committed value of this variable
        /// </summary>
        /// <returns>committed value</returns>
        public int LatestValue()
        {
            return History.OrderByDescending(h => h.Key)
                .First()
                .Value;
        }

        /// <summary>
        /// Finds the latest value of a variable at a certain time
        /// </summary>
        /// <param name="time"></param>
        /// <returns>committed value at time passed in</returns>
        public int ValueAtTime(int time)
        {
            return History.OrderByDescending(h => h.Key)
                .First(h => h.Key <= time)
                .Value;
        }

        public override string ToString()
        {
            return $"{Name}={LatestValue()}";
        }
    }
}