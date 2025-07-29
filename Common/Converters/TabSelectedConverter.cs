using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Common.Converters
{
    /// <summary>
    /// Convertisseur pour la couleur de fond des onglets sélectionnés
    /// </summary>
    public class TabSelectedConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedTab = value?.ToString();
            var tabName = parameter?.ToString();
            
            if (selectedTab == tabName)
            {
                // Couleur de fond pour onglet sélectionné
                return Application.Current.RequestedTheme == AppTheme.Dark 
                    ? Color.FromArgb("#BB86FC") 
                    : Color.FromArgb("#6200EE");
            }
            
            // Couleur transparente pour onglet non sélectionné
            return Colors.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}