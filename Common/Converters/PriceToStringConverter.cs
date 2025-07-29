using System.Globalization;
using Microsoft.Maui.Controls;

namespace Common.Converters
{/// <summary>
    /// Formate un prix pour l'affichage
    /// </summary>
    public class PriceToStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double price && price > 0)
            {
                return $"{price:F2}€";
            }
            else if (value is decimal decimalPrice && decimalPrice > 0)
            {
                return $"{decimalPrice:F2}€";
            }
            return "Prix non défini";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}