using System.Collections.Generic;
using System.Linq;
using DistributedDb.Transactions;

namespace DistributedDb.Deadlocks
{
    public class Graph
    {
        public Graph()
        {
            Nodes = new List<Node>();
        }

        public IList<Node> Nodes { get; set; }

        public void Add(Transaction vertex, IList<Transaction> neighbors = null)
        {
            var node = GetNode(vertex);

            if (neighbors != null)
            {
                AddNeighbors(node, neighbors);
            }
        }

        public void Remove(Transaction transaction)
        {
            var node = Nodes.Where(n => n.Vertex == transaction).FirstOrDefault();
            Nodes.Remove(node);

            foreach (var gnode in Nodes)
            {
                gnode.Neighbors.Remove(transaction);
            }
        }

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