namespace Common.Converters
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            bool status = (bool)value;
            string param = parameter?.ToString();

            // Si c'est pour l'archivage (parameter = "archive")
            if (param == "archive")
            {
                return status ? Color.FromArgb("#FF9800") : Color.FromArgb("#4CAF50"); // Orange si archivé, Vert sinon
            }
            
            // Pour la complétion (défaut)
            return status ? Color.FromArgb("#4CAF50") : Color.FromArgb("#FF9800"); // Vert si complet, Orange sinon
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
            => throw new NotImplementedException();
    }
}