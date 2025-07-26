using System.Windows.Input;
using BU.Services;
using DAL.DB;

namespace TravelPlannMauiApp.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IUtilisateurService _utilisateurService;
    private string _email = string.Empty;
    private string _motDePasse = string.Empty;

    public LoginViewModel() : this(null)
    {
        // Constructeur sans paramètre pour les cas où l'injection échoue
    }

    public LoginViewModel(IUtilisateurService utilisateurService = null)
    {
        // Si utilisateurService est null, on peut créer une instance temporaire ou gérer l'erreur
        _utilisateurService = utilisateurService ?? throw new InvalidOperationException("Service utilisateur non disponible");
        
        try
        {
            // Utiliser les méthodes CreateCommand du BaseViewModel
            LoginCommand = CreateCommand(LoginAsync, CanLogin);
            RegisterCommand = CreateCommand(NavigateToRegisterAsync);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la création des commandes: {ex}");
            // Créer des commandes vides pour éviter les erreurs
            LoginCommand = new Command(() => { });
            RegisterCommand = new Command(() => { });
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                // Forcer la réévaluation du CanExecute
                ((Command)LoginCommand)?.ChangeCanExecute();
            }
        }
    }

    public string MotDePasse
    {
        get => _motDePasse;
        set
        {
            if (SetProperty(ref _motDePasse, value))
            {
                // Forcer la réévaluation du CanExecute
                ((Command)LoginCommand)?.ChangeCanExecute();
            }
        }
    }

    public ICommand LoginCommand { get; private set; }
    public ICommand RegisterCommand { get; private set; }

    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(Email) && 
               !string.IsNullOrWhiteSpace(MotDePasse) && 
               !IsBusy;
    }

    private async Task LoginAsync()
    {        
        try
        {
            System.Diagnostics.Debug.WriteLine($"Tentative de connexion pour: {Email}");
            
            var utilisateur = await _utilisateurService.AuthenticateAsync(Email, MotDePasse);
            
            if (utilisateur != null)
            {
                System.Diagnostics.Debug.WriteLine("Connexion réussie");
                
                // Sauvegarder l'utilisateur connecté
                await SecureStorage.SetAsync("current_user_id", utilisateur.UtilisateurId.ToString());
                await SecureStorage.SetAsync("current_user_name", $"{utilisateur.Prenom} {utilisateur.Nom}");
                
                // Naviguer vers la page principale
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("Échec de la connexion");
                await Shell.Current.DisplayAlert("Erreur", "Identifiants incorrects", "OK");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur de connexion: {ex}");
            await HandleError(ex, "Erreur lors de la connexion");
        }
    }

    private async Task NavigateToRegisterAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Navigation vers RegisterPage");
            await Shell.Current.GoToAsync("RegisterPage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur navigation: {ex}");
            await HandleError(ex, "Erreur lors de la navigation");
        }
    }
}