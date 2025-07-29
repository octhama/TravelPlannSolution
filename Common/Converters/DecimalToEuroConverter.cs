namespace Common.Converters
{
    public class DecimalToEuroConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is decimal decimalValue)
            {
                return $"{decimalValue:F2} €";
            }
            if (value is double doubleValue)
            {
                return $"{doubleValue:F2} €";
            }
            if (value is float floatValue)
            {
                return $"{floatValue:F2} €";
            }
            if (value is int intValue)
            {
                return $"{intValue:F2} €";
            }
            
            return "0,00 €";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is string stringValue)
            {
                // Supprimer le symbole € et les espaces pour la conversion inverse
                var cleanValue = stringValue.Replace("€", "").Trim();
                if (decimal.TryParse(cleanValue, NumberStyles.Number, culture, out decimal result))
                {
                    return result;
                }
            }
            return 0m;
        }
    }
}