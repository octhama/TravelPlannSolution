using TravelPlannMauiApp.ViewModels;

namespace TravelPlannMauiApp.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}