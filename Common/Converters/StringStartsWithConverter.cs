using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Common.Converters
{
 /// <summary>
    /// Vérifie si une chaîne commence par un préfixe donné
    /// </summary>
    public class StringStartsWithConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var stringValue = value?.ToString();
            var prefix = parameter?.ToString();
            
            if (string.IsNullOrEmpty(stringValue) || string.IsNullOrEmpty(prefix))
                return false;
                
            return stringValue.StartsWith(prefix, StringComparison.OrdinalIgnoreCase);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}