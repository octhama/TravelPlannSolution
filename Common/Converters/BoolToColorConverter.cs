using System.Globalization;

namespace Common.Converters
{
    public class BoolToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool boolValue && parameter is string colorParams)
            {
                var colors = colorParams.Split(',');
                if (colors.Length == 2)
                {
                    var trueColor = colors[0].Trim();
                    var falseColor = colors[1].Trim();
                    
                    return boolValue ? Color.FromArgb(trueColor) : Color.FromArgb(falseColor);
                }
            }
            
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}