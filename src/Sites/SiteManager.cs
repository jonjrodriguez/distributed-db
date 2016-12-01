using System;
using System.Collections.Generic;
using System.Linq;
using DistributedDb.Operations;
using DistributedDb.Variables;

namespace DistributedDb.Sites
{
    public class SiteManager
    {
        public SiteManager()
        {
            var variables = new List<Variable>();
            for (int i = 1; i <= 20; i++) {
                variables.Add(new Variable(i));
            }

            Sites = new List<Site>();
            for (int i = 1; i <= 10; i++)
            {
                var data = variables
                    .Where(variable => variable.Id % 2 == 0 || i == 1 + variable.Id % 10)
                    .ToList();

                Sites.Add(new Site(i, data));    
            }
        }
        
        public IList<Site> Sites { get; set; }

        public void execute(IEnumerable<Operation> operations)
        {
            Console.WriteLine($"SiteManager: {operations.Count()} operations");
            foreach (var operation in operations)
            {
                Console.WriteLine(operation.ToString());
            }
        }
    }
}