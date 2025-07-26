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
            System.Diagnostics.Debug.WriteLine($"=== DÉBUT AUTHENTIFICATION ===");
            System.Diagnostics.Debug.WriteLine($"Email saisi: '{Email}'");
            System.Diagnostics.Debug.WriteLine($"Mot de passe saisi: '{MotDePasse}'");
            System.Diagnostics.Debug.WriteLine($"Longueur email: {Email?.Length}");
            System.Diagnostics.Debug.WriteLine($"Longueur mot de passe: {MotDePasse?.Length}");
            
            // Vérification que le service est disponible
            if (_utilisateurService == null)
            {
                System.Diagnostics.Debug.WriteLine("ERREUR: Service utilisateur null");
                await Shell.Current.DisplayAlert("Erreur", "Service d'authentification non disponible", "OK");
                return;
            }

            System.Diagnostics.Debug.WriteLine("Service utilisateur OK, appel AuthenticateAsync...");
            
            var utilisateur = await _utilisateurService.AuthenticateAsync(Email.Trim(), MotDePasse);
            
            System.Diagnostics.Debug.WriteLine($"Résultat AuthenticateAsync: {(utilisateur != null ? "Utilisateur trouvé" : "Utilisateur non trouvé")}");
            
            if (utilisateur != null)
            {
                System.Diagnostics.Debug.WriteLine($"Connexion réussie pour: {utilisateur.Prenom} {utilisateur.Nom}");
                System.Diagnostics.Debug.WriteLine($"ID utilisateur: {utilisateur.UtilisateurId}");
                
                // Sauvegarder l'utilisateur connecté avec gestion d'erreur SecureStorage
                try
                {
                    await SecureStorage.SetAsync("current_user_id", utilisateur.UtilisateurId.ToString());
                    await SecureStorage.SetAsync("current_user_name", $"{utilisateur.Prenom} {utilisateur.Nom}");
                    System.Diagnostics.Debug.WriteLine("Informations sauvegardées dans SecureStorage");
                }
                catch (Exception secureStorageEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur SecureStorage: {secureStorageEx.Message}");
                    
                    // Alternative: utiliser Preferences comme fallback
                    try
                    {
                        Preferences.Set("current_user_id", utilisateur.UtilisateurId.ToString());
                        Preferences.Set("current_user_name", $"{utilisateur.Prenom} {utilisateur.Nom}");
                        System.Diagnostics.Debug.WriteLine("Informations sauvegardées dans Preferences (fallback)");
                    }
                    catch (Exception prefEx)
                    {
                        System.Diagnostics.Debug.WriteLine($"Erreur Preferences: {prefEx.Message}");
                        await Shell.Current.DisplayAlert("Avertissement", 
                            "Connexion réussie mais impossible de sauvegarder les informations de session.", "OK");
                    }
                }
                
                System.Diagnostics.Debug.WriteLine("Navigation vers MainPage...");
                
                // Naviguer vers la page principale
                await Shell.Current.GoToAsync("//MainPage");
            }
            else
            {
                System.Diagnostics.Debug.WriteLine("=== ÉCHEC DE LA CONNEXION ===");
                System.Diagnostics.Debug.WriteLine("AuthenticateAsync a retourné null");
                
                // Test de diagnostic : essayer de récupérer tous les utilisateurs pour voir si la DB fonctionne
                try
                {
                    System.Diagnostics.Debug.WriteLine("Test de diagnostic de la base de données...");
                    var testUser = await _utilisateurService.GetByEmailAsync(Email.Trim());
                    System.Diagnostics.Debug.WriteLine($"Utilisateur trouvé par email: {testUser != null}");
                    
                    if (testUser != null)
                    {
                        System.Diagnostics.Debug.WriteLine($"Mot de passe en DB: '{testUser.MotDePasse}'");
                        System.Diagnostics.Debug.WriteLine($"Mot de passe saisi: '{MotDePasse}'");
                        System.Diagnostics.Debug.WriteLine($"Comparaison directe: {testUser.MotDePasse == MotDePasse}");
                    }
                }
                catch (Exception dbEx)
                {
                    System.Diagnostics.Debug.WriteLine($"Erreur lors du test DB: {dbEx}");
                }
                
                await Shell.Current.DisplayAlert("Erreur", 
                    $"Identifiants incorrects.\n\nEmail: {Email}\nVeuillez vérifier vos informations.", "OK");
            }
            
            System.Diagnostics.Debug.WriteLine($"=== FIN AUTHENTIFICATION ===");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"=== EXCEPTION DURANT AUTHENTIFICATION ===");
            System.Diagnostics.Debug.WriteLine($"Type: {ex.GetType().Name}");
            System.Diagnostics.Debug.WriteLine($"Message: {ex.Message}");
            System.Diagnostics.Debug.WriteLine($"Stack trace: {ex.StackTrace}");
            
            if (ex.InnerException != null)
            {
                System.Diagnostics.Debug.WriteLine($"Inner exception: {ex.InnerException.Message}");
            }
            
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

    // Méthode utilitaire pour récupérer l'ID utilisateur avec fallback
    public static async Task<string?> GetCurrentUserIdAsync()
    {
        try
        {
            // Essayer SecureStorage d'abord
            return await SecureStorage.GetAsync("current_user_id");
        }
        catch (Exception)
        {
            // Fallback vers Preferences
            return Preferences.Get("current_user_id", null);
        }
    }

    // Méthode utilitaire pour récupérer le nom utilisateur avec fallback
    public static async Task<string?> GetCurrentUserNameAsync()
    {
        try
        {
            // Essayer SecureStorage d'abord
            return await SecureStorage.GetAsync("current_user_name");
        }
        catch (Exception)
        {
            // Fallback vers Preferences
            return Preferences.Get("current_user_name", null);
        }
    }
}