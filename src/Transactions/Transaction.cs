using System.Collections.Generic;
using System.Linq;
using DistributedDb.Variables;

namespace DistributedDb.Transactions
{
    public class Transaction
    {   
        public string Name { get; set; }

        public bool IsReadOnly { get; set; }

        public IList<Variable> SnapShot { get; set; }

        public Variable ReadFromSnapShot(string variableName)
        {
            return SnapShot.FirstOrDefault(v => v.Name == variableName);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}