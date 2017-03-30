using Newtonsoft.Json;
using System;

namespace EventByte
{
    public class Event
    {
        public int EventId { get; set; }
        public string Name { get; set; }
        public string Category { get; set; }
        public DateTime StartDate { get; set; }
        public decimal Price { get; set; }
        public double Latitude { get; set; }
        public double Longitude { get; set; }
    }
}