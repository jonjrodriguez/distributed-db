using System;
using System.Collections.Generic;
using System.Linq;
using DistributedDb.Operations;

namespace DistributedDb.Transactions
{
    public class TransactionManager
    {
        public IList<Transaction> Transactions { get; set; }

        public void Execute(IEnumerable<Operation> operations)
        {
            Console.WriteLine($"TransactionManager: {operations.Count()} operations");
            foreach (var operation in operations)
            {
                Console.WriteLine(operation.ToString());
            }
        }
    }
}