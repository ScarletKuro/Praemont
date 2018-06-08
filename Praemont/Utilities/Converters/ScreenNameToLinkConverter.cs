using System;
using System.Globalization;
using System.Windows.Data;

namespace Praemont.Utilities.Converters
{
    [ValueConversion(typeof (string), typeof (string))]
    internal class ScreenNameToLinkConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return $"https://twitter.com/{value}";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}