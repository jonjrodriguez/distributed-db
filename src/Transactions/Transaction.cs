using System.Collections.Generic;
using System.Linq;
using DistributedDb.Variables;

namespace DistributedDb.Transactions
{
    public class Transaction
    {
        public Transaction()
        {
            LocalData = new List<Variable>();
        }
        
        public string Name { get; set; }

        public bool IsReadOnly { get; set; }

        public IList<Variable> LocalData { get; set; }

        public Variable ReadFromLocal(string variableName)
        {
            return LocalData.FirstOrDefault(v => v.Name == variableName);
        }

        public override string ToString()
        {
            return Name;
        }
    }
}