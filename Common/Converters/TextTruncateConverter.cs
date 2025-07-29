using System.Globalization;
using Microsoft.Maui.Controls;

namespace Common.Converters
{/// <summary>
    /// Tronque un texte à une longueur maximale
    /// </summary>
    public class TextTruncateConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var text = value?.ToString();
            if (string.IsNullOrEmpty(text))
                return string.Empty;

            var maxLength = 50; // Longueur par défaut
            if (parameter != null && int.TryParse(parameter.ToString(), out int length))
                maxLength = length;

            if (text.Length <= maxLength)
                return text;

            return text.Substring(0, maxLength - 3) + "...";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}