using System;
using DistributedDb.Sites;
using DistributedDb.Transactions;
using DistributedDb.Operations;
using System.Collections.Generic;

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
                transactionManager.execute(operations);
                Console.WriteLine(time);
                time++;
            }

            Console.WriteLine("Jobs Done.");
        }
    }
}
