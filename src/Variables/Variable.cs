namespace DistributedDb.Variables
{
    public class Variable
    {
        public Variable()
        {
        }

        public Variable(int id)
        {
            Id = id;
            Name = "x" + id;
            Value = id * 10;
            IsReplicated = id % 2 == 0;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int Value { get; set; }

        public int UpdatedValue { get; set; }

        public bool IsReplicated { get; set; }

        public override string ToString()
        {
            return $"{Name}={Value}";
        }
    }
}