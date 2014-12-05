using System;
using System.Collections.Generic;
using System.ComponentModel.Composition;
using System.Linq;
using Cafeteria.Models;
using Cafeteria.Services.Contracts;
using Cafeteria.Wpf.Infrastructure;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Cafeteria.Wpf.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class DashboardViewModel : ViewModelBase
    {
        public DateTime MinDate { get; set; }

        public DateTime MaxDate { get; set; }

        public List<Table> Tables { get; set; }
        public List<Reservation> Reservations { get; set; }

        private readonly ITableReservationService _reservationService;

        public string ViewTitle{get { return "Dashboard"; }}

        private readonly IEventAggregator _eventAggregator;

        [ImportingConstructor]
        public DashboardViewModel(ITableReservationService reservationService, IEventAggregator eventAggregator)
        {
            _reservationService = reservationService;
            _eventAggregator = eventAggregator;
            
            var now = DateTime.Now;
            MinDate = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0);
            MaxDate = new DateTime(now.Year, now.Month, now.Day, 22, 0, 0);

            LoadAllData();
            SubscribeToEvents();
        }

        private void SubscribeToEvents()
        {
            _eventAggregator.GetEvent<ReservationAddOrEditEvent>().Subscribe(ReservationUpdated);
            _eventAggregator.GetEvent<ReservationDeletedEvent>().Subscribe(ReservationDeleted);
        }

        private void ReservationDeleted(Reservation reservation)
        {
            var r = Reservations.FirstOrDefault(x => x.ReservationId == reservation.ReservationId);
            if (r != null) Reservations.Remove(r);
        }

        private void ReservationUpdated(Reservation reservation)
        {
            var r = Reservations.FirstOrDefault(x => x.ReservationId == reservation.ReservationId);

            if(r == null)
            {
                var newr = new Reservation
                {
                    FromTime = reservation.FromTime, 
                    ToTime = reservation.ToTime,
                    ReservationId = reservation.ReservationId,
                    NumberOfPeople = reservation.NumberOfPeople,
                    TableNumber = reservation.TableNumber,
                    ReservedFor = reservation.ReservedFor
                };
                Reservations.Add(newr);
            }
            else
            {
                r.NumberOfPeople = reservation.NumberOfPeople;
                r.ReservedFor = reservation.ReservedFor;
                r.TableNumber = reservation.TableNumber;
                r.ToTime = reservation.ToTime;
                r.FromTime = reservation.FromTime;
            }
        }

        private void LoadAllData()
        {
            Tables = _reservationService.GetAllTables();
            Reservations = _reservationService.GetAllReservations();
        }
    }
}
