﻿namespace Rift.Data.Models
{
    public class ScheduledEvent
    {
        public uint Id { get; set; }
        public int DayId { get; set; }
        public int EventId { get; set; }
        public int Hour { get; set; }
        public int Minute { get; set; }
    }
}
