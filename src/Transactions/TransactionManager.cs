using System;
using System.Collections.Generic;
using DistributedDb.Operations;

namespace DistributedDb.Transactions
{
    public class TransactionManager
    {
        public IList<Transaction> Transactions { get; set; }

        public void execute(IList<Operation> operations)
        {
            foreach (var operation in operations)
            {
                Console.WriteLine(operation.ToString());
            }
        }
    }
}