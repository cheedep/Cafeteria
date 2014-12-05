using System.Collections.Generic;
using Cafeteria.Models;

namespace Cafeteria.Services.Contracts
{
    public interface ITableReservationService
    {
        List<Reservation> GetAllReservations();
        List<Reservation> GetAllReservationsSortedByTableAndTime(); 

        bool DeleteReservation(Reservation reservation);
        bool AddOrUpdateReservation(Reservation reservation);

        bool CheckReservation(Reservation reservation);

        List<Table> GetAllTables();
    }
}
