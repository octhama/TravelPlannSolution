using System.Globalization;


#if NET8_0_OR_GREATER && !NET8_0_ANDROID && !NET8_0_IOS && !NET8_0_MACCATALYST && !NET8_0_WINDOWS
// For standard .NET, create a simple interface
namespace Common.Converters
{
    public interface IValueConverter
    {
        object Convert(object value, Type targetType, object parameter, CultureInfo culture);
        object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture);
    }

    public class BoolToArchiveTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? "Désarchiver" : "Archiver";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
#else
// For MAUI platforms, use the MAUI IValueConverter
using Microsoft.Maui.Controls;

namespace Common.Converters
{
    public class BoolToArchiveTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
            => (bool)value ? "Désarchiver" : "Archiver";

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}
#endif