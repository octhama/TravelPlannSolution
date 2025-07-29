using System.Globalization;
using Microsoft.Maui.Controls;

namespace Common.Converters
{/// <summary>
    /// Convertit une collection en nombre d'éléments
    /// </summary>
    public class CollectionCountConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is System.Collections.ICollection collection)
                return collection.Count;
            if (value is System.Collections.IEnumerable enumerable)
                return enumerable.Cast<object>().Count();
            return 0;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}