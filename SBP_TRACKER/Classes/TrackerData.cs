using System;
using System.Collections.Generic;

namespace SBP_TRACKER
{
    public class TrackerVar
    {
        public string Slave { get; set; }

        public string Name { get; set; }

        public double Value { get; set; }

        public string? Unit { get; set; }

    }

    public class TrackerData
    {
        public int ID { get; set; }

        public DateTime? Lastregister { get; set; }

        public List<TrackerVar>? Listvar { get; set; }
    }
}
