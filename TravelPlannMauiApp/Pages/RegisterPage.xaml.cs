using TravelPlannMauiApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TravelPlannMauiApp.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
        
        // Obtenir le ViewModel via l'injection de dépendances
        var serviceProvider = Handler?.MauiContext?.Services ?? 
                             (Application.Current as App)?.Handler?.MauiContext?.Services;
        
        if (serviceProvider != null)
        {
            var viewModel = serviceProvider.GetService<RegisterViewModel>();
            BindingContext = viewModel;
        }
    }

    // Garder le constructeur avec paramètre pour compatibilité
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }
}