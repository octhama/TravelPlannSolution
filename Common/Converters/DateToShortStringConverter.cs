using System.Globalization;
using Microsoft.Maui.Controls;

namespace Common.Converters
{/// <summary>
    /// Formate une date pour l'affichage compact
    /// </summary>
    public class DateToShortStringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is DateTime date)
            {
                if (date.Year == DateTime.Now.Year)
                    return date.ToString("dd/MM");
                return date.ToString("dd/MM/yy");
            }
            return "Date inconnue";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}