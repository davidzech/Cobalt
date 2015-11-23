using System;
using System.Globalization;
using System.Windows.Data;

namespace Cobalt.Converters
{
    [ValueConversion(typeof(object), typeof(bool))]
    public class ObjectToBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
            {
                return false;
            }
            else return true;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null;
        }
    }
}