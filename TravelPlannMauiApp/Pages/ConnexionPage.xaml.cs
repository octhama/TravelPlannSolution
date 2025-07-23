using TravelPlannMauiApp.ViewModels;

namespace TravelPlannMauiApp.Pages
{
    public partial class ConnexionPage : ContentPage
    {
        public ConnexionPage()
        {
            InitializeComponent();
            BindingContext = Handler.MauiContext.Services.GetService<AuthViewModel>();
        }
    }
}