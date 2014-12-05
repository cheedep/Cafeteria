using System;
using System.Globalization;
using System.Windows.Data;

namespace Cafeteria.Wpf.Infrastructure
{
    public class DateTimeIncrementConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime && parameter is int)
            {
                var d = (DateTime)value;
                var i = (int)parameter;
                return d.AddMinutes(i);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
