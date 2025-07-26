using TravelPlannMauiApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TravelPlannMauiApp.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
        
        // Obtenir le ViewModel via l'injection de dépendances
        var serviceProvider = Handler?.MauiContext?.Services ?? 
                             (Application.Current as App)?.Handler?.MauiContext?.Services;
        
        if (serviceProvider != null)
        {
            var viewModel = serviceProvider.GetService<LoginViewModel>();
            BindingContext = viewModel;
        }
    }

    // Garder le constructeur avec paramètre pour compatibilité
    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}