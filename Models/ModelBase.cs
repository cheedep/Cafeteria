using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using FluentValidation;
using FluentValidation.Results;
using Models.Annotations;

namespace Cafeteria.Models
{
    public class ModelBase : INotifyPropertyChanged, IDataErrorInfo
    {
        protected IValidator Validator = null;
        
        public ModelBase()
        {
            Validator = GetValidator();
            Validate();
        }

        #region INotifyPropertyChanged Members
        public event PropertyChangedEventHandler PropertyChanged;

        [NotifyPropertyChangedInvocator]
        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChangedEventHandler handler = PropertyChanged;
            if (handler != null) handler(this, new PropertyChangedEventArgs(propertyName));
            Validate();
        }
        #endregion

        #region Validation

        protected virtual IValidator GetValidator()
        {
            return null;
        }

        protected IEnumerable<ValidationFailure> _validationErrors = null;
        public IEnumerable<ValidationFailure> ValidationErrors
        {
            get { return _validationErrors; }
        }

        public void Validate()
        {
            if (Validator != null)
            {
                ValidationResult results = Validator.Validate(this);
                _validationErrors = results.Errors;
            }
        }

        public virtual bool IsValid
        {
            get
            {
                if (_validationErrors != null && _validationErrors.Count() > 0)
                    return false;
                else
                    return true;
            }
        }

        #endregion

        #region IDataErrorInfo members

        string IDataErrorInfo.Error
        {
            get { return string.Empty; }
        }

        string IDataErrorInfo.this[string columnName]
        {
            get
            {
                var errors = new StringBuilder();

                if (_validationErrors != null && _validationErrors.Count() > 0)
                {
                    foreach (ValidationFailure validationError in _validationErrors)
                    {
                        if (validationError.PropertyName == columnName)
                            errors.AppendLine(validationError.ErrorMessage);
                    }
                }

                return errors.ToString();
            }
        }

        #endregion
    }
}
