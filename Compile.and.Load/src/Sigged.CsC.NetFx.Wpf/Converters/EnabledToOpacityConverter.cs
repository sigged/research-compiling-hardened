using System;
using System.Globalization;
using System.Windows.Data;

namespace Sigged.CsCNetFx.Wpf.Converters
{
    public class EnabledToOpacityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool)
            {
                return (bool)value ? 1D : 0.5D;
            }
            return 1D;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
