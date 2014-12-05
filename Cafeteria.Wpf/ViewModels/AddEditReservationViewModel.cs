using System;
using System.ComponentModel;
using Cafeteria.Models;
using Cafeteria.Services.Contracts;
using Cafeteria.Wpf.Infrastructure;

namespace Cafeteria.Wpf.ViewModels
{
    public class AddEditReservationViewModel
    {
        //Copy of the editing model
        public Reservation Reservation { get; set; }
        private DelegateCommand<object> _saveCommand;
        private DelegateCommand<object> _cancelCommand;

        private bool _isAdd;

        private const string AddTitle = "New Reservation";
        private const string UpdateTitle = "Edit Reservation";

        public string ViewTitle { get; set; }

        public TimeSpan TimeInterval { get; set; }
        public TimeSpan OpeningTime { get; set; }
        public TimeSpan LastReservationTime { get; set; }
        public TimeSpan ClosingTime { get; set; }

        public DateTime? FromTimeMin { get; set; }
        public DateTime? FromTimeMax { get; set; }
        public DateTime? ToTimeMax { get; set; }

        private readonly ITableReservationService _service;

        public AddEditReservationViewModel(ITableReservationService service, Reservation reservation)
        {
            _service = service;

            TimeInterval = TimeSpan.FromMinutes(1);

            OpeningTime = new TimeSpan(10, 0, 0);
            ClosingTime = new TimeSpan(22, 0, 0);
            LastReservationTime = new TimeSpan(21, 30, 0);

            var now = DateTime.Now;
            FromTimeMin = new DateTime(now.Year, now.Month, now.Day, 10, 0, 0);
            FromTimeMax = new DateTime(now.Year, now.Month, now.Day, 21, 30, 0);
            ToTimeMax = new DateTime(now.Year, now.Month, now.Day, 22, 0, 0);

            if (reservation == null)
            {
                _isAdd = true;
                Reservation = new Reservation();
                if (now > FromTimeMin && now < FromTimeMax)
                {
                    Reservation.ToTime = now.AddMinutes(15);
                    Reservation.FromTime = now;
                }
                else
                {
                    Reservation.FromTime = FromTimeMin.Value;
                    Reservation.ToTime = FromTimeMin.Value.AddMinutes(15);
                }
                ViewTitle = AddTitle;
            }
            else
            {
                _isAdd = false;
                Reservation = new Reservation
                {
                    ReservationId = reservation.ReservationId,
                    ToTime = reservation.ToTime,
                    FromTime = reservation.FromTime,
                    NumberOfPeople = reservation.NumberOfPeople,
                    ReservedFor = reservation.ReservedFor,
                    TableNumber = reservation.TableNumber
                };
                ViewTitle = UpdateTitle;
            }

            Reservation.PropertyChanged += ReservationPropertyChanged;
        }

        private void ReservationPropertyChanged(object sender, PropertyChangedEventArgs args)
        {
            if (args.PropertyName == "NumberOfPeople" || args.PropertyName == "FromTime" || args.PropertyName == "ToTime")
            {
                _service.CheckReservation(Reservation);
            }
        }

        #region Commands

        public DelegateCommand<object> SaveCommand
        {
            get { return _saveCommand ?? (_saveCommand = new DelegateCommand<object>(Save, CanSave)); }
        }

        private bool CanSave(object obj)
        {
            Reservation.Validate();
            return Reservation.IsValid;
        }

        public event EventHandler CancelReservationUpdate;
        public event EventHandler<ReservationEventArgs> ReservationUpdated;

        private void Save(object obj)
        {
            if (_service.AddOrUpdateReservation(Reservation))
            {
                if (ReservationUpdated != null)
                    ReservationUpdated(this, new ReservationEventArgs(Reservation, _isAdd));
                Reservation.PropertyChanged -= ReservationPropertyChanged;
            }
            //else set error message
        }

        public DelegateCommand<object> CancelCommand
        {
            get { return _cancelCommand ?? (_cancelCommand = new DelegateCommand<object>(Cancel)); }
        }

        private void Cancel(object obj)
        {
            if (CancelReservationUpdate != null)
                CancelReservationUpdate(this, EventArgs.Empty);
            Reservation.PropertyChanged -= ReservationPropertyChanged;
        }

        #endregion
    }
}