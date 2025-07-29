using System.Windows.Input;
using BU.Services;

namespace TravelPlannMauiApp.ViewModels;

public class RegisterViewModel : BaseViewModel
{
    private readonly IUtilisateurService _utilisateurService; // Le _ c'est pour indiquer que c'est un champ privé et non une propriété publique
    private string _nom = string.Empty;
    private string _prenom = string.Empty;
    private string _email = string.Empty;
    private string _motDePasse = string.Empty;
    private string _confirmMotDePasse = string.Empty;

    public RegisterViewModel() : this(null)
    {
    }

    public RegisterViewModel(IUtilisateurService utilisateurService = null)
    {
        _utilisateurService = utilisateurService ?? throw new InvalidOperationException("Service utilisateur non disponible");
        
        RegisterCommand = CreateCommand(RegisterAsync, CanRegister);
        BackToLoginCommand = CreateCommand(BackToLoginAsync);
    }

    public string Nom
    {
        get => _nom;
        set
        {
            if (SetProperty(ref _nom, value))
            {
                ((Command)RegisterCommand)?.ChangeCanExecute();
            }
        }
    }

    public string Prenom
    {
        get => _prenom;
        set
        {
            if (SetProperty(ref _prenom, value))
            {
                ((Command)RegisterCommand)?.ChangeCanExecute();
            }
        }
    }

    public string Email
    {
        get => _email;
        set
        {
            if (SetProperty(ref _email, value))
            {
                ((Command)RegisterCommand)?.ChangeCanExecute();
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
                ((Command)RegisterCommand)?.ChangeCanExecute();
            }
        }
    }

    public string ConfirmMotDePasse
    {
        get => _confirmMotDePasse;
        set
        {
            if (SetProperty(ref _confirmMotDePasse, value))
            {
                ((Command)RegisterCommand)?.ChangeCanExecute();
            }
        }
    }

    public ICommand RegisterCommand { get; private set; }
    public ICommand BackToLoginCommand { get; private set; }

    private bool CanRegister()
    {
        return !string.IsNullOrWhiteSpace(Nom) &&
               !string.IsNullOrWhiteSpace(Prenom) &&
               !string.IsNullOrWhiteSpace(Email) &&
               !string.IsNullOrWhiteSpace(MotDePasse) &&
               MotDePasse == ConfirmMotDePasse &&
               MotDePasse.Length >= 6 &&
               !IsBusy;
    }

    private async Task RegisterAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine($"Tentative de création de compte pour: {Email}");
            
            var utilisateur = await _utilisateurService.CreateUserAsync(Nom, Prenom, Email, MotDePasse);
            
            System.Diagnostics.Debug.WriteLine("Compte créé avec succès");
            await Shell.Current.DisplayAlert("Succès", "Compte créé avec succès!", "OK");
            await Shell.Current.GoToAsync("..");
        }
        catch (InvalidOperationException ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur validation: {ex.Message}");
            await Shell.Current.DisplayAlert("Erreur", ex.Message, "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur création compte: {ex}");
            await HandleError(ex, "Erreur lors de la création du compte");
        }
    }

    private async Task BackToLoginAsync()
    {
        try
        {
            System.Diagnostics.Debug.WriteLine("Retour vers LoginPage");
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur navigation retour: {ex}");
            await HandleError(ex, "Erreur lors de la navigation");
        }
    }
}