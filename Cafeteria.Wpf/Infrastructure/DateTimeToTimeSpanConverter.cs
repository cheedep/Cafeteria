using System;
using System.Globalization;
using System.Windows.Data;

namespace Cafeteria.Wpf.Infrastructure
{
    public class DateTimeToTimeSpanConverter:IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is DateTime && parameter is int)
            {
                var d = (DateTime) value;
                var i = (int)parameter;
                var t = d.AddMinutes(i);
                return new TimeSpan(t.Hour, t.Minute, t.Second);
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
