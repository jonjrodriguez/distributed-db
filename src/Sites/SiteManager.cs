using System;
using System.Collections.Generic;
using System.Linq;
using DistributedDb.Operations;
using DistributedDb.Variables;

namespace DistributedDb.Sites
{
    public class SiteManager
    {
        public SiteManager(Clock clock)
        {
            Clock = clock;

            var variables = new List<Variable>();
            for (int i = 1; i <= 20; i++) {
                variables.Add(new Variable(i, clock.Time));
            }

            Sites = new List<Site>();
            for (int i = 1; i <= 10; i++)
            {
                var data = variables
                    .Where(variable => variable.Id % 2 == 0 || i == 1 + variable.Id % 10)
                    .Select(variable => new Variable(variable.Id, clock.Time))
                    .ToList();

                Sites.Add(new Site(i, data));    
            }
        }

        public Clock Clock { get; set; }
        
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
                    case OperationType.Fail:
                        Fail((int) operation.Site);
                        break;
                    case OperationType.Recover:
                        Recover((int) operation.Site);
                        break;
                    default:
                        Console.WriteLine(operation.ToString());
                        break;
                }
            }
        }

        public void Fail(int siteId)
        {
            var site = Sites.FirstOrDefault(s => s.Id == siteId);

            if (site == null)
            {
                Console.WriteLine($"Site {siteId} doesn't exist.");
                Environment.Exit(1);
            }

            site.Fail();
        }

        public void Recover(int siteId)
        {
            var site = Sites.FirstOrDefault(s => s.Id == siteId);

            if (site == null)
            {
                Console.WriteLine($"Site {siteId} doesn't exist.");
                Environment.Exit(1);
            }

            site.Recover(Clock.Time);
        }

        public void Dump(int? siteId, string variable)
        {
            var sites = siteId == null ? Sites : Sites.Where(s => s.Id == siteId);
            sites = string.IsNullOrWhiteSpace(variable) ? sites : SitesWithVariable(variable);

            foreach (var site in sites)
            {
                Console.WriteLine(site.Dump(variable));
            }
        }

        public List<Site> SitesWithVariable(string variable, SiteState? state = null)
        {
            var sites = Sites.Where(s => s.Data.Any(d => d.Name == variable));

            if (state != null)
            {
                sites = sites.Where(s => s.State == state);
            }

            return sites.ToList();
        }
    }
}