using System;
using System.Globalization;
using System.Windows.Data;
using Praemont.Utilities.System;

namespace Praemont.Utilities.Converters
{
    public class Win7FontFamilyConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return "Segoe UI Symbol";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}