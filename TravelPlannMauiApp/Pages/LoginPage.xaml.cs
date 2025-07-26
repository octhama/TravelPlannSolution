using TravelPlannMauiApp.ViewModels;

namespace TravelPlannMauiApp.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}