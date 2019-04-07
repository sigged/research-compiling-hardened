using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Sigged.Repl.NetFx.Wpf.Converters
{
    public class BooleanToStatusColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Color defaultColor = Colors.WhiteSmoke;
            if (value is bool && parameter is Color)
            {
                return ((bool)value) ? (Color)parameter : defaultColor;
            }
            return defaultColor;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}

