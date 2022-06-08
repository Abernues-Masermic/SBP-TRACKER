using System;


namespace SBP_TRACKER
{
    public class TrackerState
    {
        public int ID { get; set; }
        public string? Name { get; set; }
        public bool? IsRunning { get; set; }
        public bool? StateUpdated { get; set; }
        public DateTime? LastConnection { get; set; }

    }
}
