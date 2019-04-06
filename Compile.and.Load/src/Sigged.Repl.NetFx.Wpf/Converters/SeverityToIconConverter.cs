using Microsoft.CodeAnalysis;
using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media.Imaging;

namespace Sigged.Repl.NetFx.Wpf.Converters
{
    public class SeverityToIconConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is DiagnosticSeverity)
            {
                var severity = (DiagnosticSeverity)value;
                switch (severity)
                {
                    case DiagnosticSeverity.Error:
                        return new BitmapImage(new Uri(@"pack://application:,,,/icons/error.png", UriKind.RelativeOrAbsolute));
                    case DiagnosticSeverity.Warning:
                        return new BitmapImage(new Uri(@"pack://application:,,,/icons/warning.png", UriKind.RelativeOrAbsolute));
                    case DiagnosticSeverity.Info:
                        return new BitmapImage(new Uri(@"pack://application:,,,/icons/info.png", UriKind.RelativeOrAbsolute));
                    default:
                        return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
