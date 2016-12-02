namespace DistributedDb.Transactions
{
    public class Transaction
    {
        public string Name { get; set; }

        public bool IsReadOnly { get; set; }

        public override string ToString()
        {
            return $"Transaction: {Name}; Read-Only: {IsReadOnly}";
        }
    }
}