using System;
using System.Globalization;
using System.Windows.Data;

namespace Praemont.Utilities.Converters
{
    [ValueConversion(typeof (int), typeof (string))]
    internal class NumberValueCommaConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var number = (int)value;
            return number.ToString("n0");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}