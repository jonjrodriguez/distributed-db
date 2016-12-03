using System.Collections.Generic;
using System.Linq;

namespace DistributedDb.Variables
{
    public class Variable
    {
        public Variable()
        {
        }

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

        public Dictionary<int, int> History { get; set; }

        public int NewValue { get; set; }

        public bool IsReplicated { get; set; }

        public bool Readable { get; set; }

        public int LatestValue()
        {
            return History.OrderByDescending(h => h.Key)
                .First()
                .Value;
        }

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