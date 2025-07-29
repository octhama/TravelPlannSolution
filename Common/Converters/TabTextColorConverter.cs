using System;
using System.Globalization;
using Microsoft.Maui.Controls;

namespace Common.Converters
{

    /// <summary>
    /// Convertisseur pour la couleur du texte des onglets
    /// </summary>
    public class TabTextColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var selectedTab = value?.ToString();
            var tabName = parameter?.ToString();

            if (selectedTab == tabName)
            {
                // Texte blanc pour onglet sélectionné
                return Colors.White;
            }

            // Couleur accent pour onglet non sélectionné
            return Application.Current.RequestedTheme == AppTheme.Dark
                ? Color.FromArgb("#BB86FC")
                : Color.FromArgb("#6200EE");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}