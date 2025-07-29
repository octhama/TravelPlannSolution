using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;

namespace Common.Converters
{
    /// <summary>
    /// Convertit une chaîne en booléen (true si non vide)
    /// </summary>
    public class StringToBoolConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !string.IsNullOrWhiteSpace(value?.ToString());
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}