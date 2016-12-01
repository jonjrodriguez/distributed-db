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

    public class Operation
    {
        public OperationType? Type { get; set; }

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