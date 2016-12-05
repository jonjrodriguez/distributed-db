namespace DistributedDb
{
    /// <summary>
    /// Manages global time across application
    /// </summary>
    public class Clock
    {
        public Clock()
        {
            Time = 0;
        }

        public int Time { get; set; }

        /// <summary>
        /// Increments global time
        /// </summary>
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