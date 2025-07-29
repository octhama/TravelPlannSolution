using System.Globalization;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Graphics;
using System;

namespace Common.Converters
{
    public class EtatToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return (value as string) switch
            {
                "Validé" => Color.FromArgb("#4CAF50"), // Vert
                "Archivé" => Color.FromArgb("#FF9800"), // Orange
                _ => Colors.Transparent
            };
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}