using System.Globalization;
using Microsoft.Maui.Controls;

namespace Common.Converters
{
    public class BindingContextConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            // This converter is used to pass the binding context as command parameter
            // The value is the item, parameter is the command
            return parameter;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
