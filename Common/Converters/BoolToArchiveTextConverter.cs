using System.Globalization;
using Microsoft.Maui.Controls;

namespace Common.Converters
{
    // BoolToArchiveTextConverter.cs
    public class BoolToArchiveTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? "Désarchiver" : "Archiver";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }

}