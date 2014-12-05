using System;
using FluentValidation;

namespace Cafeteria.Models
{
    public class Reservation : ModelBase
    {
        private int _reservationId;
        private int? _tableNumber;
        private string _reservedFor;
        private int? _numberOfPeople;
        private DateTime _fromTime;
        private DateTime _toTime;
        private string _message;
        private MessageType _messageType;

        public int ReservationId
        {
            get { return _reservationId; }
            set
            {
                if (_reservationId != value)
                {
                    _reservationId = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? TableNumber
        {
            get { return _tableNumber; }
            set
            {
                if (_tableNumber != value)
                {
                    _tableNumber = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ReservedFor
        {
            get { return _reservedFor; }
            set
            {
                if (_reservedFor != value)
                {
                    _reservedFor = value;
                    OnPropertyChanged();
                }
            }
        }

        public int? NumberOfPeople
        {
            get { return _numberOfPeople; }
            set
            {
                if (_numberOfPeople != value)
                {
                    _numberOfPeople = value;
                    OnPropertyChanged();
                }
            }
        }

        public DateTime FromTime
        {
            get { return _fromTime; }
            set
            {
                if (_fromTime != value)
                {
                    _fromTime = value;
                    if (_fromTime >= _toTime)
                    {
                        ToTime = _fromTime.AddMinutes(1);
                    }
                    OnPropertyChanged();
                }
            }
        }

        public DateTime ToTime
        {
            get { return _toTime; }
            set
            {
                if (_toTime != value)
                {
                    _toTime = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Message
        {
            get { return _message; }
            set
            {
                if (_message != value)
                {
                    _message = value;
                    OnPropertyChanged();
                }
            }
        }

        public MessageType MessageType
        {
            get { return _messageType; }
            set
            {
                if(_messageType != value)
                {
                    _messageType = value;
                    OnPropertyChanged();
                }
            }
        }

        protected override IValidator GetValidator()
        {
            return new ReservationValidator();
        }
    }

    class ReservationValidator : AbstractValidator<Reservation>
    {
        public ReservationValidator()
        {
            RuleFor(x => x.ReservedFor).NotEmpty().Length(0, 10);
            RuleFor(x => x.NumberOfPeople).NotEmpty().GreaterThan(0);
            RuleFor(x => x.FromTime).NotEmpty();
            RuleFor(x => x.ToTime).NotEmpty().GreaterThan(x => x.FromTime);
            RuleFor(x => x.TableNumber).NotEmpty().GreaterThan(0);
        }
    }
}
