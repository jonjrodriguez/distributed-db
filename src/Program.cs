using System;
using DistributedDb.Sites;
using DistributedDb.Transactions;
using DistributedDb.Operations;
using System.Collections.Generic;
using System.Linq;

namespace DistributedDb
{
    public class Program
    {
        public static void Main(string[] args)
        {
            var clock = new Clock();
            var siteManager = new SiteManager(clock);
            var transactionManager = new TransactionManager(clock, siteManager);

            var parser = new InputParser(args);

            List<Operation> operations;
            while ((operations = parser.GetInstruction()) != null)
            {
                clock.Tick();
                Console.WriteLine(clock.ToString());
                siteManager.Execute(operations.Where(op => Operation.SiteOperations.Contains(op.Type)));
                transactionManager.Execute(operations.Where(op => !Operation.SiteOperations.Contains(op.Type)));
                Console.WriteLine();
            }

            Console.WriteLine("Jobs Done.");
        }
    }
}
