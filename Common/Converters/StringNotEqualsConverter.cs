using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Common.Converters
{/// <summary>
    /// Vérifie si une chaîne n'est pas égale à un paramètre
    /// </summary>
    public class StringNotEqualsConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value?.ToString();
            var compareValue = parameter?.ToString();
            
            return stringValue != compareValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}