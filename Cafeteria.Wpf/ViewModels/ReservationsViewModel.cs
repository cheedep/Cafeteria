using System;
using System.Collections.ObjectModel;
using System.ComponentModel.Composition;
using System.Linq;
using Cafeteria.Models;
using Cafeteria.Services.Contracts;
using Cafeteria.Wpf.Infrastructure;
using Cafeteria.Wpf.Views;
using MahApps.Metro.Controls.Dialogs;
using Microsoft.Practices.Prism.PubSubEvents;

namespace Cafeteria.Wpf.ViewModels
{
    [Export]
    [PartCreationPolicy(CreationPolicy.NonShared)]
    public class ReservationsViewModel : ViewModelBase
    {
        private ITableReservationService _reservationService;
        private readonly IUiService _uiService;

        private AddEditReservationDialog _addEditReservationDialog = null;

        public string ViewTitle { get { return "Reservations"; } }

        private IEventAggregator _eventAggregator;

        [ImportingConstructor]
        public ReservationsViewModel(ITableReservationService reservationService, IEventAggregator eventAggregator)
        {
            _reservationService = reservationService;
            _eventAggregator = eventAggregator;
            _uiService = new UiService();

            InitializeReservations();
        }

        private void InitializeReservations()
        {
            var resList = _reservationService.GetAllReservationsSortedByTableAndTime();
            if (resList == null || resList.Count == 0)
                _reservations = new ObservableCollection<Reservation>();
            else
                _reservations = new ObservableCollection<Reservation>(resList);
        }

        private ObservableCollection<Reservation> _reservations;
        private DelegateCommand<Reservation> _editReservationCommand;
        private DelegateCommand<object> _addReservationCommand;
        private AddEditReservationViewModel _currentReservationViewModel;
        private DelegateCommand<Reservation> _deleteReservationCommand;

        public ObservableCollection<Reservation> Reservations
        {
            get { return _reservations; }
            set
            {
                if (_reservations != value)
                {
                    _reservations = value;
                    OnPropertyChanged();
                }
            }
        }

        public AddEditReservationViewModel CurrentReservationViewModel
        {
            get { return _currentReservationViewModel; }
            set
            {
                if (_currentReservationViewModel != value)
                {
                    _currentReservationViewModel = value;
                    OnPropertyChanged();
                }
            }
        }

        #region Commands

        public DelegateCommand<object> AddReservationCommand
        {
            get { return _addReservationCommand ?? (_addReservationCommand = new DelegateCommand<object>(AddNewReservation)); }
        }

        private void AddNewReservation(object obj)
        {
            CurrentReservationViewModel = new AddEditReservationViewModel(_reservationService, null);

            CurrentReservationViewModel.ReservationUpdated += OnReservationUpdated;
            CurrentReservationViewModel.CancelReservationUpdate += OnCancelReservationUpdate;


            _addEditReservationDialog = _uiService.GetView<AddEditReservationDialog>();

            _addEditReservationDialog.DataContext = this;
            _addEditReservationDialog.ShowDialog();
        }

        public DelegateCommand<Reservation> EditReservationCommand
        {
            get { return _editReservationCommand ?? (_editReservationCommand = new DelegateCommand<Reservation>(EditReservation)); }
        }

        private void EditReservation(Reservation reservation)
        {
            CurrentReservationViewModel = new AddEditReservationViewModel(_reservationService, reservation);

            CurrentReservationViewModel.ReservationUpdated += OnReservationUpdated;
            CurrentReservationViewModel.CancelReservationUpdate += OnCancelReservationUpdate;

            _addEditReservationDialog = _uiService.GetView<AddEditReservationDialog>();
            _addEditReservationDialog.DataContext = this;
            _addEditReservationDialog.ShowDialog();
        }

        public DelegateCommand<Reservation> DeleteReservationCommand
        {
            get { return _deleteReservationCommand ?? (_deleteReservationCommand = new DelegateCommand<Reservation>(DeleteReservation)); }
        }

        private async void DeleteReservation(Reservation reservation)
        {
            var result = await _uiService.ShowYesNoQuestionAsync(string.Format("Delete reservation for {0}", reservation.ReservedFor), "Confirm");
            if (result == MessageDialogResult.Affirmative)
                if (_reservationService.DeleteReservation(reservation))
                {
                    Reservations.Remove(reservation);
                    _eventAggregator.GetEvent<ReservationDeletedEvent>().Publish(reservation);
                }
        }

        private void OnCancelReservationUpdate(object sender, EventArgs eventArgs)
        {
            CurrentReservationViewModel.ReservationUpdated -= OnReservationUpdated;
            CurrentReservationViewModel.CancelReservationUpdate -= OnCancelReservationUpdate;

            CurrentReservationViewModel = null;

            _addEditReservationDialog.DialogResult = false;
            _addEditReservationDialog = null;
        }

        private void OnReservationUpdated(object sender, ReservationEventArgs reservationEventArgs)
        {
            if (reservationEventArgs.IsAdded)
            {
                if (Reservations.Any())
                {
                    var newRes = reservationEventArgs.Reservation;
                    for (int i = 0; i < Reservations.Count; i++)
                    {
                        var r = Reservations[i];
                        if (r.TableNumber > newRes.TableNumber)
                        {
                            Reservations.Insert(i, newRes);
                            break;
                        }
                        if (r.TableNumber == newRes.TableNumber && r.FromTime > newRes.FromTime)
                        {
                            Reservations.Insert(i, newRes);
                            break;
                        }
                        if (i == Reservations.Count - 1)
                        {
                            Reservations.Add(newRes);
                            break;
                        }
                    }
                }
                else
                {
                    _reservations.Add(reservationEventArgs.Reservation);
                }
            }
            else
            {
                var editedReservation =
                    _reservations.FirstOrDefault(x => x.ReservationId == reservationEventArgs.Reservation.ReservationId);
                if (editedReservation != null)
                {
                    editedReservation.ReservedFor = reservationEventArgs.Reservation.ReservedFor;
                    editedReservation.TableNumber = reservationEventArgs.Reservation.TableNumber;
                    editedReservation.NumberOfPeople = reservationEventArgs.Reservation.NumberOfPeople;
                    editedReservation.FromTime = reservationEventArgs.Reservation.FromTime;
                    editedReservation.ToTime = reservationEventArgs.Reservation.ToTime;
                }
            }

            CurrentReservationViewModel.ReservationUpdated -= OnReservationUpdated;
            CurrentReservationViewModel.CancelReservationUpdate -= OnCancelReservationUpdate;

            CurrentReservationViewModel = null;

            _addEditReservationDialog.DialogResult = true;
            _addEditReservationDialog = null;

            _eventAggregator.GetEvent<ReservationAddOrEditEvent>().Publish(reservationEventArgs.Reservation);
        }

        #endregion
    }
}
