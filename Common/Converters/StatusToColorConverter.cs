
using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Common.Converters
{
    public class StatusToColorConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        return (bool)value ? 
            (parameter == null ? Color.FromArgb("#4CAF50") : Color.FromArgb("#FF9800")) :
            (parameter == null ? Color.FromArgb("#FF9800") : Color.FromArgb("#4CAF50"));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotImplementedException();
}
}