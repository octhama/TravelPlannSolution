using System.Windows.Input;
using BU.Services;
using DAL.DB;

namespace TravelPlannMauiApp.ViewModels;

public class LoginViewModel : BaseViewModel
{
    private readonly IUtilisateurService _utilisateurService;
    private string _email = string.Empty;
    private string _motDePasse = string.Empty;

    public LoginViewModel(IUtilisateurService utilisateurService)
    {
        _utilisateurService = utilisateurService;
        LoginCommand = CreateCommand(LoginAsync, CanLogin);
        RegisterCommand = CreateCommand(NavigateToRegisterAsync);
    }

    public string Email
    {
        get => _email;
        set
        {
            SetProperty(ref _email, value);
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }

    public string MotDePasse
    {
        get => _motDePasse;
        set
        {
            SetProperty(ref _motDePasse, value);
            ((Command)LoginCommand).ChangeCanExecute();
        }
    }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }

    private bool CanLogin()
    {
        return !string.IsNullOrWhiteSpace(Email) && !string.IsNullOrWhiteSpace(MotDePasse);
    }

    private async Task LoginAsync()
    {
        try
        {
            var utilisateur = await _utilisateurService.AuthenticateAsync(Email, MotDePasse);
            
            if (utilisateur != null)
            {
                // Sauvegarder l'utilisateur connecté
                await SecureStorage.SetAsync("current_user_id", utilisateur.UtilisateurId.ToString());
                await SecureStorage.SetAsync("current_user_name", $"{utilisateur.Prenom} {utilisateur.Nom}");
                
                // Naviguer vers la page principale
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                await Shell.Current.DisplayAlert("Erreur", "Identifiants incorrects", "OK");
            }
        }
        catch (Exception ex)
        {
            await HandleError(ex, "Erreur lors de la connexion");
        }
    }

    private async Task NavigateToRegisterAsync()
    {
        await Shell.Current.GoToAsync("RegisterPage");
    }
}
