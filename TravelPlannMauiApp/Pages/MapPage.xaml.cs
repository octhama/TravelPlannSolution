using Microsoft.Maui.Controls;

namespace TravelPlannMauiApp.Pages
{
    public class MapPage : ContentPage
    {
        public MapPage()
        {
            Content = new Label
            {
                Text = "Map Page",
                HorizontalOptions = LayoutOptions.Center,
                VerticalOptions = LayoutOptions.Center
            };
        }
    }
}