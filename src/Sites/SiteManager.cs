using System.Collections.Generic;
using System.Linq;
using DistributedDb.Operations;
using DistributedDb.Variables;

namespace DistributedDb.Sites
{
    /// <summary>
    /// Handles operations that are site related (fail/recover/dump)
    /// Makes available utility function to retrieve sites
    /// </summary>
    public class SiteManager
    {
        /// <summary>
        /// Instantiates all sites and variables
        /// </summary>
        /// <param name="clock"></param>
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

        private Clock Clock { get; set; }
        
        /// <summary>
        /// List of all sites
        /// </summary>
        private IList<Site> Sites { get; set; }

        /// <summary>
        /// Executes site specific operations
        /// Fail/Recover/Dump
        /// </summary>
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
                        Logger.Fail($"Operation '{operation}' is not supported.");
                        break;
                }
            }
        }

        /// <summary>
        /// Fails the given site
        /// </summary>
        /// <param name="siteId"></param>
        private void Fail(int siteId)
        {
            var site = Sites
                .Where(s => s.State == SiteState.Stable)
                .FirstOrDefault(s => s.Id == siteId);

            if (site == null)
            {
                Logger.Fail($"Site {siteId} has already failed.");
            }

            site.Fail();
        }

        /// <summary>
        /// Recovers the given site
        /// </summary>
        /// <param name="siteId"></param>
        private void Recover(int siteId)
        {
            var site = Sites
                .Where(s => s.State == SiteState.Fail)
                .FirstOrDefault(s => s.Id == siteId);

            if (site == null)
            {
                Logger.Fail($"Site {siteId} is not in a failed state.");
            }

            site.Recover(Clock.Time);
        }

        /// <summary>
        /// Prints out the site data
        /// No params prints out all sites and all variables
        /// If siteId given, prints out all variables at that site
        /// If variable name given, prints out that variable at all sites
        /// </summary>
        /// <param name="siteId"></param>
        /// <param name="variable"></param>
        private void Dump(int? siteId, string variable)
        {
            var sites = siteId == null ? Sites : Sites.Where(s => s.Id == siteId);
            sites = string.IsNullOrWhiteSpace(variable) ? sites : SitesWithVariable(variable);

            var dumping = siteId != null ? siteId.ToString() : variable;
            Logger.Write($"{Clock.ToString()} Dump({dumping}):");
            foreach (var site in sites)
            {
                Logger.Write(site.Dump(variable));
            }
        }

        /// <summary>
        /// Utility function to find sites where a variable is stored
        /// </summary>
        /// <param name="variable"></param>
        /// <param name="state">Optional state to limit the sites to</param>
        /// <returns></returns>
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