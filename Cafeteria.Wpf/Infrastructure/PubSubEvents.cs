using Cafeteria.Models;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Cafeteria.Wpf.Infrastructure
{
    public sealed class ReservationAddOrEditEvent : PubSubEvent<Reservation> { }
    public sealed class ReservationDeletedEvent : PubSubEvent<Reservation> { }
}
