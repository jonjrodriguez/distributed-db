using System.Collections.Generic;
using System.Linq;
using DistributedDb.Transactions;

namespace DistributedDb.Deadlocks
{
    public class Node
    {
        public Node(Transaction vertex)
        {
            Vertex = vertex;

            Neighbors = new List<Transaction>();
        }

        public Transaction Vertex { get; set; }

        public List<Transaction> Neighbors { get; set; }

        public bool Contains(Transaction neighbor)
        {
            return Neighbors.Any(n => n == neighbor);
        }
    }
}