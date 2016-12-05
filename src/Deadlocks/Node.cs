using System.Collections.Generic;
using System.Linq;
using DistributedDb.Transactions;

namespace DistributedDb.Deadlocks
{
    /// <summary>
    /// Representation of a graph Node
    /// Each node has a vertex and a list of it's neighbors
    /// </summary>
    public class Node
    {
        public Node(Transaction vertex)
        {
            Vertex = vertex;

            Neighbors = new List<Transaction>();
        }

        public Transaction Vertex { get; set; }

        public List<Transaction> Neighbors { get; set; }

        /// <summary>
        /// Returns whether this node is connection to the given Transaction
        /// </summary>
        /// <param name="neighbor"></param>
        public bool Contains(Transaction neighbor)
        {
            return Neighbors.Any(n => n == neighbor);
        }
    }
}