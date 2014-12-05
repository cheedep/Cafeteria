using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Cafeteria.Models;
using Cafeteria.Services.Contracts;

namespace Cafeteria.Services
{
    [Export(typeof(ITableReservationService))]
    public class TableReservationService : ITableReservationService
    {
        private readonly ITablesRepository _tablesRepository;
        private int? _biggestTableSize;

        private readonly DateTime _startTime;
        private readonly DateTime _finalReservationTime;
        private readonly DateTime _closeTime;

        [ImportingConstructor]
        public TableReservationService(ITablesRepository tablesRepository)
        {
            _tablesRepository = tablesRepository;

            var now = DateTime.Now;
            _startTime = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0);
            _closeTime = new DateTime(now.Year, now.Month, now.Day, 22, 0, 0);
            _finalReservationTime = new DateTime(now.Year, now.Month, now.Day, 21, 30, 0);
        }

        List<Reservation> ITableReservationService.GetAllReservations()
        {
            return _tablesRepository.GetAllReservations();
        }

        public List<Reservation> GetAllReservationsSortedByTableAndTime()
        {
            return _tablesRepository.GetAllReservationsSortedByTableAndTime();
        }


        public bool DeleteReservation(Reservation reservation)
        {
            return _tablesRepository.DeleteReservation(reservation);
        }

        public bool AddOrUpdateReservation(Reservation reservation)
        {
            return _tablesRepository.AddOrUpdateReservation(reservation);
        }

        private int? BiggestTableSize
        {
            get { return _biggestTableSize ?? (_biggestTableSize = _tablesRepository.GetBiggestTableSize()); }
        }

        public bool CheckReservation(Reservation reservation)
        {
            if (!reservation.NumberOfPeople.HasValue || reservation.FromTime > reservation.ToTime)
            {
                reservation.Message = null;
                return false;
            }

            if (reservation.NumberOfPeople > BiggestTableSize)
            {
                reservation.TableNumber = null;
                reservation.Message = string.Format("Sorry, Cannot seat more than {0} people.", BiggestTableSize);
                reservation.MessageType = MessageType.Failure;
                return false;
            }

            //int bestTable = _tablesRepository.GetBestTableAvailable(reservation);

            //if (bestTable == 0)
            //{
            //    reservation.Suggestion = "No Tables are available for selected time";
            //    return false;
            //}

            //reservation.TableNumber = bestTable;
            //reservation.Suggestion = string.Format("Success!! Table {0} available", bestTable);

            var bestTable = GetBestTableAvailability(reservation); //_tablesRepository.GetBestTableAvailability(reservation);
            if (bestTable == null || bestTable.TableId == 0)
            {
                reservation.TableNumber = null;
                reservation.Message = "No Tables are available for selected time and duration";
                reservation.MessageType = MessageType.Failure;
                return false;
            }

            if (!bestTable.IsMatch)
            {
                reservation.TableNumber = null;
                reservation.Message = string.Format("Best Availability Table {0} From {1} to {2}",
                                                    bestTable.TableId,
                                                    bestTable.FromT.ToString("t"),
                                                    bestTable.ToT.ToString("t"));
                reservation.MessageType = MessageType.Suggestion;
                return false;
            }

            reservation.TableNumber = bestTable.TableId;
            reservation.Message = string.Format("Success!! Table {0} available", bestTable.TableId);
            reservation.MessageType = MessageType.Success;
            return true;
        }

        private TableAvailability GetBestTableAvailability(Reservation r)
        {
            var tables = _tablesRepository.GetAllDomainTablesSortedByMaxOccupancy();
            var reservations = _tablesRepository.GetAllDomainReservations();

            var bestT = new TableAvailability();
            double timeRequested = r.ToTime.Subtract(r.FromTime).TotalMinutes;

            foreach (var table in tables.Where(x => x.MaxOccupancy >= r.NumberOfPeople))
            {
                var rs = reservations.Where(x => x.TableId == table.Id).OrderBy(x => x.FromTime).ToList();

                if (rs.Count == 0)
                {
                    return new TableAvailability { TableId = table.Id, IsMatch = true };
                }

                bool hasOverlap = false;
                for (int i = 0; i < rs.Count; i++)
                {
                    var s = rs[i];
                    //s.Id != r.ReservationId for when editing a reservation
                    if (s.Id != r.ReservationId && s.FromTime < r.ToTime && s.ToTime > r.FromTime) // overlap
                    {
                        hasOverlap = true;

                        for (int j = i; j < rs.Count; j++)
                        {
                            var current = rs[j];
                            if (j == rs.Count - 1) //last reservation
                            {
                                if (current.ToTime <= _finalReservationTime)
                                {
                                    UpdateBestTable(bestT, table.Id, current.ToTime, _closeTime, timeRequested);
                                }
                            }
                            else
                            {
                                var next = rs[j + 1];
                                UpdateBestTable(bestT, table.Id, current.ToTime, next.FromTime, timeRequested);
                            }
                        }
                        break;
                    }
                }
                if (!hasOverlap)
                {
                    return new TableAvailability { TableId = table.Id, IsMatch = true };
                }
            }

            return bestT;
        }

        private void UpdateBestTable(TableAvailability bestT, int id, DateTime fromT, DateTime toT, double timeRequested)
        {
            double timeNew = toT.Subtract(fromT).TotalMinutes; //total minutes duration of this period

            if (bestT.TableId == 0)
            {
                if (timeNew > 1)
                {
                    bestT.TableId = id;
                    bestT.FromT = fromT;
                    bestT.ToT = toT;
                    bestT.Duration = toT.Subtract(fromT).TotalMinutes;
                }
            }
            else
            {
                if ((fromT < bestT.FromT && (timeNew > timeRequested || timeNew >= bestT.Duration)) ||
                        (bestT.Duration < timeRequested && timeNew > bestT.Duration))
                {
                    bestT.TableId = id;
                    bestT.FromT = fromT;
                    bestT.ToT = toT;
                    bestT.Duration = timeNew;
                }
            }
        }


        public List<Table> GetAllTables()
        {
            return _tablesRepository.GetAllTables();
        }
    }
}
