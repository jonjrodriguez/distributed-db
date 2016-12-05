namespace DistributedDb.Operations
{
    public enum OperationType
    {
        Begin,
        BeginRO,
        Read,
        Write,
        Dump,
        End,
        Fail,
        Recover
    }

    /// <summary>
    /// Representation of the possible operations
    /// </summary>
    public class Operation
    {
        public static readonly OperationType[] SiteOperations =
        {
            OperationType.Dump,
            OperationType.Fail,
            OperationType.Recover
        };

        public OperationType Type { get; set; }

        public string Transaction { get; set; }

        public int? Site { get; set; }

        public string Variable { get; set; }

        public int? WriteValue { get; set; }

        public override string ToString()
        {
            return $"Type: {Type}, Transaction: {Transaction}, Site: {Site}, Variable: {Variable}, WriteValue: {WriteValue}";
        }
    }
}