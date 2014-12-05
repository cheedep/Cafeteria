using System.Collections.Generic;
using System.Linq;
using Reservation = Cafeteria.Services.Domain.Reservation;
using Table = Cafeteria.Services.Domain.Table;

namespace Cafeteria.Services.Contracts
{
    public interface ITablesRepository
    {
        IQueryable<Table> QueryTables();
        IQueryable<Reservation> QueryReservations();
        bool AddOrUpdateReservation(Models.Reservation reservation);
        bool DeleteReservation(Models.Reservation reservation);

        List<Models.Reservation> GetAllReservations();
        List<Models.Reservation> GetAllReservationsSortedByTableAndTime();

        int GetBiggestTableSize();
      
        List<Table> GetAllDomainTablesSortedByMaxOccupancy();
        List<Reservation> GetAllDomainReservations();

        List<Models.Table> GetAllTables();
    }
}
