using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;

namespace Common.Converters
{
    public class SelectedItemBackgroundConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value != null && parameter != null && value == parameter)
            {
                return Color.FromArgb("#E3F2FD"); // Light blue for selected
            }
            return Color.FromArgb("#F5F5F5"); // Light gray for unselected
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
