using System;
using System.Collections.Generic;
using System.Linq;
using DistributedDb.Sites;
using DistributedDb.Transactions;

namespace DistributedDb.Deadlocks
{
    public class DeadlockManager
    {
        private SiteManager SiteManager;
        private Clock Clock;

        public DeadlockManager(SiteManager siteManager, Clock clock)
        {
            SiteManager = siteManager;
            Clock = clock;
        }

        public void DetectDeadlocks(IList<Transaction> transactions)
        {            
            var blockingGraph = ConstructGraph(transactions.Where(t => t.Active()));

            HashSet<Transaction> cycle;
            while ((cycle = GetCycle(blockingGraph)) != null)
            {
                var youngest = KillYoungest(cycle);
                blockingGraph.Remove(youngest);
            } 
        }

        private Graph ConstructGraph(IEnumerable<Transaction> transactions)
        {
            var graph = new Graph();
            foreach (var transaction in transactions)
            {
                if (!transaction.IsWaiting()) {
                    graph.Add(transaction);
                    continue;
                }

                var variable = transaction.OperationBuffer.Variable;
                var stableSites = SiteManager.SitesWithVariable(variable, SiteState.Stable);
                foreach (var site in stableSites)
                {
                    var waitingFor = site.GetBlockingTransactions(transaction, variable);
                    graph.Add(transaction, waitingFor);
                }
            }

            return graph;
        }

        private HashSet<Transaction> GetCycle(Graph graph)
        {
            foreach (var node in graph.Nodes)
            {
                var visited = new HashSet<Transaction>();
                visited.Add(node.Vertex);
                
                if (HasCycle(graph, node, visited))
                {
                    return visited;
                }
            }

            return null;
        }

        private bool HasCycle(Graph graph, Node node, HashSet<Transaction> visited)
        {
            foreach (var neighbor in node.Neighbors)
            {
                if (visited.Contains(neighbor))
                {
                    return true;
                }
                
                visited.Add(neighbor);
                var nextNode = graph.GetNode(neighbor);
                if (HasCycle(graph, nextNode, visited))
                {
                    return true;
                }
            }

            return false;
        }

        private Transaction KillYoungest(HashSet<Transaction> cycle)
        {
            var youngest = cycle.OrderByDescending(t => t.StartTime).First();
            
            youngest.EndTime = Clock.Time;
            Logger.Write($"{Clock.ToString()} Transaction {youngest.ToString()} aborted (deadlock).");
            youngest.State = TransactionState.Aborted;
            foreach (var site in youngest.GetStableSites())
            {
                site.ClearLocks(youngest);
            }

            return youngest;
        }
    }
}