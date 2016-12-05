using System.Collections.Generic;
using System.Linq;
using DistributedDb.Transactions;

namespace DistributedDb.Deadlocks
{
    /// <summary>
    /// Representation of a Graph
    /// Used in deadlock detection
    /// </summary>
    public class Graph
    {
        public Graph()
        {
            Nodes = new List<Node>();
        }

        /// <summary>
        /// List of nodes in the graph
        /// </summary>
        public IList<Node> Nodes { get; set; }

        /// <summary>
        /// Adds a transaction to the graph with any of it's connected Transactions
        /// </summary>
        /// <param name="vertex"></param>
        /// <param name="neighbors"></param>
        public void Add(Transaction vertex, IList<Transaction> neighbors = null)
        {
            var node = GetNode(vertex);

            if (neighbors != null)
            {
                AddNeighbors(node, neighbors);
            }
        }

        /// <summary>
        /// Removes a transaction from the graph
        /// Also removes it as a neighbor from any other node
        /// </summary>
        /// <param name="transaction"></param>
        public void Remove(Transaction transaction)
        {
            var node = Nodes.Where(n => n.Vertex == transaction).FirstOrDefault();
            Nodes.Remove(node);

            foreach (var gnode in Nodes)
            {
                gnode.Neighbors.Remove(transaction);
            }
        }

        /// <summary>
        /// Gets the node from the given transaction
        /// Creates the node if it doesn't exists
        /// </summary>
        /// <param name="vertex"></param>
        public Node GetNode(Transaction vertex)
        {
            var node = Nodes.FirstOrDefault(n => n.Vertex == vertex);

            if (node == null)
            {
                node = new Node(vertex);
                Nodes.Add(node);
            }

            return node;
        }

        /// <summary>
        /// Adds each transaction as a neighbor to the given node
        /// </summary>
        /// <param name="node"></param>
        private void AddNeighbors(Node node, IList<Transaction> neighbors)
        {
            foreach (var neighbor in neighbors)
            {
                if (!node.Contains(neighbor))
                {
                    node.Neighbors.Add(neighbor);
                }
            }
        }
    }
}