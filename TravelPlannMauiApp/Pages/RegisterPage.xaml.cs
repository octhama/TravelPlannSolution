using TravelPlannMauiApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TravelPlannMauiApp.Pages;

public partial class RegisterPage : ContentPage
{
    public RegisterPage()
    {
        InitializeComponent();
    }

    // Garder le constructeur avec paramètre pour compatibilité
    public RegisterPage(RegisterViewModel viewModel)
    {
        InitializeComponent();
        BindingContext = viewModel;
    }

    protected override void OnHandlerChanged()
    {
        base.OnHandlerChanged();
        
        if (BindingContext == null && Handler?.MauiContext?.Services != null)
        {
            SetupViewModel();
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        if (BindingContext == null)
        {
            System.Diagnostics.Debug.WriteLine("BindingContext null dans RegisterPage OnAppearing");
            SetupViewModel();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"RegisterPage BindingContext: {BindingContext.GetType().Name}");
        }
    }

    private void SetupViewModel()
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;
            
            if (serviceProvider != null)
            {
                var viewModel = serviceProvider.GetService<RegisterViewModel>();
                if (viewModel != null)
                {
                    BindingContext = viewModel;
                    System.Diagnostics.Debug.WriteLine("RegisterViewModel créé avec succès via DI");
                    return;
                }
                
                var utilisateurService = serviceProvider.GetService<BU.Services.IUtilisateurService>();
                if (utilisateurService != null)
                {
                    BindingContext = new RegisterViewModel(utilisateurService);
                    System.Diagnostics.Debug.WriteLine("RegisterViewModel créé manuellement");
                    return;
                }
            }
            
            System.Diagnostics.Debug.WriteLine("Impossible de créer RegisterViewModel - services non disponibles");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la création du RegisterViewModel: {ex}");
        }
    }
}