namespace DistributedDb
{
    public class Clock
    {
        public Clock()
        {
            Time = 0;
        }

        public int Time { get; set; }

        public void Tick()
        {
            Time++;
        }

        public override string ToString()
        {
            return $"Time {Time}:";
        }
    }
}