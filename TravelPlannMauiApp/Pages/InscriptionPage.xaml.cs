using TravelPlannMauiApp.ViewModels;

namespace TravelPlannMauiApp.Pages
{
    public partial class InscriptionPage : ContentPage
    {
        public InscriptionPage()
        {
            InitializeComponent();
            BindingContext = Handler.MauiContext.Services.GetService<AuthViewModel>();
        }
    }
}