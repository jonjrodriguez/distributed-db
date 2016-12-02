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

        public void Execute(IEnumerable<Operation> operations)
        {
            foreach (var operation in operations)
            {
                switch (operation.Type)
                {
                    case OperationType.Dump:
                        Dump(operation.Site, operation.Variable);
                        break;
                    default:
                        Console.WriteLine(operation.ToString());
                        break;
                }
            }
        }

        public void Dump(int? siteId, string variable)
        {
            var sites = siteId == null ? Sites : Sites.Where(s => s.Id == siteId);
            sites = string.IsNullOrWhiteSpace(variable) ? sites : SitesWithVariable(variable);

            foreach (var site in sites)
            {
                Console.WriteLine(site.ToString(variable));
            }
        }

        public List<Site> SitesWithVariable(string variable)
        {
            return Sites.Where(s => s.Data.Any(d => d.Name == variable)).ToList();
        }
    }
}