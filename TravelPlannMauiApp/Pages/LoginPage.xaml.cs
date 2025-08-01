using TravelPlannMauiApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TravelPlannMauiApp.Pages;

public partial class LoginPage : ContentPage
{
    public LoginPage()
    {
        InitializeComponent();
    }

    // Garde le constructeur avec paramètre pour compatibilité
    public LoginPage(LoginViewModel viewModel)
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
        
        // Vérifier que le BindingContext est bien défini
        if (BindingContext == null)
        {
            System.Diagnostics.Debug.WriteLine("BindingContext null dans OnAppearing, tentative de création");
            SetupViewModel();
        }
        else
        {
            System.Diagnostics.Debug.WriteLine($"BindingContext disponible: {BindingContext.GetType().Name}");
        }
    }

    private void OnLoginButtonClicked(object sender, EventArgs e)
    {
        System.Diagnostics.Debug.WriteLine("Bouton login cliqué");
        System.Diagnostics.Debug.WriteLine($"BindingContext: {BindingContext?.GetType().Name}");
        if (BindingContext is LoginViewModel vm)
        {
            System.Diagnostics.Debug.WriteLine($"Email: {vm.Email}, MotDePasse: {vm.MotDePasse}");
            System.Diagnostics.Debug.WriteLine($"LoginCommand: {vm.LoginCommand}");
        }
    }
    private void SetupViewModel()
    {
        try
        {
            var serviceProvider = Handler?.MauiContext?.Services;

            if (serviceProvider != null)
            {
                var viewModel = serviceProvider.GetService<LoginViewModel>();
                if (viewModel != null)
                {
                    BindingContext = viewModel;
                    System.Diagnostics.Debug.WriteLine("LoginViewModel créé avec succès via DI");
                    return;
                }

                System.Diagnostics.Debug.WriteLine("LoginViewModel non trouvé via DI, création manuelle");
                var utilisateurService = serviceProvider.GetService<BU.Services.IUtilisateurService>();
                if (utilisateurService != null)
                {
                    BindingContext = new LoginViewModel(utilisateurService);
                    System.Diagnostics.Debug.WriteLine("LoginViewModel créé manuellement");
                    return;
                }
            }

            System.Diagnostics.Debug.WriteLine("Impossible de créer le ViewModel - services non disponibles");
            BindingContext = new EmptyLoginViewModel();
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la création du ViewModel: {ex}");
            BindingContext = new EmptyLoginViewModel();
        }
    }
}