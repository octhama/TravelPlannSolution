using BU.Services;
using Microsoft.Maui.Controls;

namespace TravelPlannMauiApp.Services
{
    public class NavigationService : INavigationService
    {
        // Existing methods

        public async Task NavigateToAsync(string route)
        {
            await Shell.Current.GoToAsync(route);
        }
    }