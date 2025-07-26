using System.Windows.Input;
using BU.Services;

namespace TravelPlannMauiApp.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly IUtilisateurService _utilisateurService;
    private string _nom = string.Empty;
    private string _prenom = string.Empty;
    private string _email = string.Empty;
    private string _motDePasse = string.Empty;
    private string _confirmMotDePasse = string.Empty;

    public RegisterViewModel(IUtilisateurService utilisateurService)
    {
        _utilisateurService = utilisateurService;
        RegisterCommand = CreateCommand(RegisterAsync, CanRegister);
        BackToLoginCommand = CreateCommand(BackToLoginAsync);
    }

    public string Nom
    {
        get => _nom;
        set
        {
            SetProperty(ref _nom, value);
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public string Prenom
    {
        get => _prenom;
        set
        {
            SetProperty(ref _prenom, value);
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            SetProperty(ref _email, value);
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public string MotDePasse
    {
        get => _motDePasse;
        set
        {
            SetProperty(ref _motDePasse, value);
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public string ConfirmMotDePasse
    {
        get => _confirmMotDePasse;
        set
        {
            SetProperty(ref _confirmMotDePasse, value);
            ((Command)RegisterCommand).ChangeCanExecute();
        }
    }

    public ICommand RegisterCommand { get; }
    public ICommand BackToLoginCommand { get; }

    private bool CanRegister()
    {
        return !string.IsNullOrWhiteSpace(Nom) &&
               !string.IsNullOrWhiteSpace(Prenom) &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(MotDePasse) &&
               MotDePasse == ConfirmMotDePasse &&
               MotDePasse.Length >= 6;
    }

    private async Task RegisterAsync()
    {
        try
        {
            var utilisateur = await _utilisateurService.CreateUserAsync(Nom, Prenom, Email, MotDePasse);
            
            await Shell.Current.DisplayAlert("Succès", "Compte créé avec succès!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (InvalidOperationException ex)
        {
            await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            await HandleError(ex, "Erreur lors de la création du compte");
        }
    }

    private async Task BackToLoginAsync()
    {
        await Shell.Current.GoToAsync("..");
    }
}
