using System;

namespace Cafeteria.Models
{
    public class TableAvailability
    {
        public int TableId { get; set; }
        public DateTime FromT { get; set; }
        public DateTime ToT { get; set; }
        public double Duration { get; set; }

        public bool IsMatch { get; set; }
    }
}
