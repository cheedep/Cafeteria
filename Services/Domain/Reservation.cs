using System;

namespace Cafeteria.Services.Domain
{
    public class Reservation
    {
        public int Id { get; set; }
        public string For { get; set; }
        public int PartySize { get; set; }
        public int TableId { get; set; }
        public DateTime FromTime { get; set; }
        public DateTime ToTime { get; set; }
    }
}
