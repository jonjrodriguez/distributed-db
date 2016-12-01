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
            var time = 1;
            var siteManager = new SiteManager();
            var transactionManager = new TransactionManager();

            var parser = new InputParser(args);

            List<Operation> operations;
            while ((operations = parser.GetInstruction()) != null)
            {
                Console.WriteLine($"Time: {time}");
                siteManager.execute(operations.Where(op => Operation.SiteOperations.Contains(op.Type)));
                transactionManager.execute(operations.Where(op => !Operation.SiteOperations.Contains(op.Type)));
                Console.WriteLine();
                
                time++;
            }

            Console.WriteLine("Jobs Done.");
        }
    }
}
