namespace DistributedDb.Variables
{
    public class Variable
    {
        public Variable(int id)
        {
            Id = id;
            Name = "x" + id;
            Value = id * 10;
        }

        public int Id { get; set; }

        public string Name { get; set; }

        public int Value { get; set; }

        public bool IsReplicated => Id % 2 == 0;

        public override string ToString()
        {
            return $"{Name}:{Value}";
        }
    }
}