using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using BU.Services;
using DAL.DB;

namespace TravelPlannMauiApp.ViewModels;

public class SettingsViewModel : INotifyPropertyChanged
{
    private readonly ISettingsService _settingsService;
    private readonly ISessionService _sessionService;
    private readonly IUtilisateurService _utilisateurService;
    
    private SettingsModel _settings;
    private Utilisateur _currentUser;
    private string _nom = string.Empty;
    private string _prenom = string.Empty;
    private string _email = string.Empty;
    private string _currentPassword = string.Empty;
    private string _newPassword = string.Empty;
    private string _confirmPassword = string.Empty;
    private bool _isBusy;

    // Event for navigation requests
    public event EventHandler<string> NavigationRequested;

    public SettingsViewModel(ISettingsService settingsService, ISessionService sessionService, IUtilisateurService utilisateurService)
    {
        _settingsService = settingsService;
        _sessionService = sessionService;
        _utilisateurService = utilisateurService;
        _settings = new SettingsModel();

        LoadSettingsCommand = new Command(async () => await LoadSettingsAsync());
        SaveSettingsCommand = new Command(async () => await SaveSettingsAsync(), CanSaveSettings);
        SaveUserInfoCommand = new Command(async () => await SaveUserInfoAsync(), CanSaveUserInfo);
        ChangePasswordCommand = new Command(async () => await ChangePasswordAsync(), CanChangePassword);
        LogoutCommand = new Command(async () => await LogoutAsync());
    }

    #region Properties
    public bool IsBusy
    {
        get => _isBusy;
        set
        {
            if (SetProperty(ref _isBusy, value))
            {
                RefreshCanExecute();
            }
        }
    }

    public string Nom
    {
        get => _nom;
        set
        {
            if (SetProperty(ref _nom, value))
            {
                ((Command)SaveUserInfoCommand)?.ChangeCanExecute();
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
                ((Command)SaveUserInfoCommand)?.ChangeCanExecute();
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
                ((Command)SaveUserInfoCommand)?.ChangeCanExecute();
            }
        }
    }

    public string CurrentPassword
    {
        get => _currentPassword;
        set
        {
            if (SetProperty(ref _currentPassword, value))
            {
                ((Command)ChangePasswordCommand)?.ChangeCanExecute();
            }
        }
    }

    public string NewPassword
    {
        get => _newPassword;
        set
        {
            if (SetProperty(ref _newPassword, value))
            {
                ((Command)ChangePasswordCommand)?.ChangeCanExecute();
            }
        }
    }

    public string ConfirmPassword
    {
        get => _confirmPassword;
        set
        {
            if (SetProperty(ref _confirmPassword, value))
            {
                ((Command)ChangePasswordCommand)?.ChangeCanExecute();
            }
        }
    }

    public string Language
    {
        get => _settings?.Language ?? "fr";
        set
        {
            if (_settings != null && _settings.Language != value)
            {
                _settings.Language = value;
                OnPropertyChanged();
                ((Command)SaveSettingsCommand)?.ChangeCanExecute();
            }
        }
    }

    public string Theme
    {
        get => _settings?.Theme ?? "Light";
        set
        {
            if (_settings != null && _settings.Theme != value)
            {
                _settings.Theme = value;
                OnPropertyChanged();
                ((Command)SaveSettingsCommand)?.ChangeCanExecute();
            }
        }
    }

    public bool NotificationsEnabled
    {
        get => _settings?.NotificationsEnabled ?? true;
        set
        {
            if (_settings != null && _settings.NotificationsEnabled != value)
            {
                _settings.NotificationsEnabled = value;
                OnPropertyChanged();
                ((Command)SaveSettingsCommand)?.ChangeCanExecute();
            }
        }
    }

    public bool LocationEnabled
    {
        get => _settings?.LocationEnabled ?? true;
        set
        {
            if (_settings != null && _settings.LocationEnabled != value)
            {
                _settings.LocationEnabled = value;
                OnPropertyChanged();
                ((Command)SaveSettingsCommand)?.ChangeCanExecute();
            }
        }
    }

    public string Currency
    {
        get => _settings?.Currency ?? "EUR";
        set
        {
            if (_settings != null && _settings.Currency != value)
            {
                _settings.Currency = value;
                OnPropertyChanged();
                ((Command)SaveSettingsCommand)?.ChangeCanExecute();
            }
        }
    }

    public string UserDisplayName => $"{Prenom} {Nom}";
    public int TotalPoints => _currentUser?.PointsRecompenses ?? 0;
    #endregion

    #region Commands
    public ICommand LoadSettingsCommand { get; }
    public ICommand SaveSettingsCommand { get; }
    public ICommand SaveUserInfoCommand { get; }
    public ICommand ChangePasswordCommand { get; }
    public ICommand LogoutCommand { get; }
    #endregion

    #region Navigation Helper
    protected virtual void OnNavigationRequested(string route)
    {
        NavigationRequested?.Invoke(this, route);
    }
    #endregion

    #region Command Methods
    private async Task LoadSettingsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            System.Diagnostics.Debug.WriteLine("=== CHARGEMENT PARAMÈTRES ===");

            // Charger les paramètres de l'application
            _settings = await _settingsService.GetSettingsAsync();
            System.Diagnostics.Debug.WriteLine($"Paramètres chargés: {_settings.Language}, {_settings.Theme}");

            // Charger les informations utilisateur
            await LoadUserInfoAsync();

            // Notifier les changements
            OnPropertyChanged(nameof(Language));
            OnPropertyChanged(nameof(Theme));
            OnPropertyChanged(nameof(NotificationsEnabled));
            OnPropertyChanged(nameof(LocationEnabled));
            OnPropertyChanged(nameof(Currency));
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur chargement paramètres: {ex}");
            await Shell.Current.DisplayAlert("Erreur", "Impossible de charger les paramètres", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LoadUserInfoAsync()
    {
        try
        {
            var userId = await _sessionService.GetCurrentUserIdAsync();
            if (userId.HasValue)
            {
                _currentUser = await _utilisateurService.GetByIdAsync(userId.Value);
                if (_currentUser != null)
                {
                    Nom = _currentUser.Nom;
                    Prenom = _currentUser.Prenom;
                    Email = _currentUser.Email;

                    OnPropertyChanged(nameof(UserDisplayName));
                    OnPropertyChanged(nameof(TotalPoints));

                    System.Diagnostics.Debug.WriteLine($"Infos utilisateur chargées: {UserDisplayName}");
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur chargement infos utilisateur: {ex}");
        }
    }

    private async Task SaveSettingsAsync()
    {
        if (IsBusy) return;
        IsBusy = true;

        try
        {
            System.Diagnostics.Debug.WriteLine("Sauvegarde des paramètres...");
            await _settingsService.SaveSettingsAsync(_settings);
            await Shell.Current.DisplayAlert("Succès", "Paramètres sauvegardés", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur sauvegarde paramètres: {ex}");
            await Shell.Current.DisplayAlert("Erreur", "Impossible de sauvegarder les paramètres", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task SaveUserInfoAsync()
    {
        if (IsBusy || _currentUser == null) return;
        IsBusy = true;

        try
        {
            System.Diagnostics.Debug.WriteLine("Sauvegarde des informations utilisateur...");

            // Vérifier si l'email a changé et s'il n'est pas déjà utilisé
            if (_currentUser.Email != Email)
            {
                var emailExists = await _utilisateurService.EmailExistsAsync(Email);
                if (emailExists)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Cette adresse email est déjà utilisée", "OK");
                    return;
                }
            }

            // Mettre à jour les informations
            _currentUser.Nom = Nom;
            _currentUser.Prenom = Prenom;
            _currentUser.Email = Email;

            await _utilisateurService.UpdateAsync(_currentUser);

            // Mettre à jour la session avec le nouveau nom
            await _sessionService.SetCurrentUserAsync(_currentUser.UtilisateurId, $"{Prenom} {Nom}");

            OnPropertyChanged(nameof(UserDisplayName));
            await Shell.Current.DisplayAlert("Succès", "Informations mises à jour", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur sauvegarde infos utilisateur: {ex}");
            await Shell.Current.DisplayAlert("Erreur", "Impossible de mettre à jour les informations", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ChangePasswordAsync()
    {
        if (IsBusy || _currentUser == null) return;
        IsBusy = true;

        try
        {
            System.Diagnostics.Debug.WriteLine("Changement de mot de passe...");

            // Vérifier le mot de passe actuel
            var authenticatedUser = await _utilisateurService.AuthenticateAsync(_currentUser.Email, CurrentPassword);
            if (authenticatedUser == null)
            {
                await Shell.Current.DisplayAlert("Erreur", "Mot de passe actuel incorrect", "OK");
                return;
            }

            // Mettre à jour le mot de passe (le service se charge du hashage)
            _currentUser.MotDePasse = NewPassword; // Le service devra hasher ce mot de passe
            await _utilisateurService.UpdateAsync(_currentUser);

            // Réinitialiser les champs
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmPassword = string.Empty;

            await Shell.Current.DisplayAlert("Succès", "Mot de passe modifié avec succès", "OK");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur changement mot de passe: {ex}");
            await Shell.Current.DisplayAlert("Erreur", "Impossible de changer le mot de passe", "OK");
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task LogoutAsync()
    {
        try
        {
            var result = await Shell.Current.DisplayAlert(
                "Déconnexion", 
                "Êtes-vous sûr de vouloir vous déconnecter ?", 
                "Oui", "Non");

            if (result)
            {
                System.Diagnostics.Debug.WriteLine("Déconnexion utilisateur...");
                await _sessionService.ClearSessionAsync();
                
                // Use NavigationRequested event instead of direct navigation
                OnNavigationRequested("//LoginPage");
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur déconnexion: {ex}");
            await Shell.Current.DisplayAlert("Erreur", "Erreur lors de la déconnexion", "OK");
        }
    }
    #endregion

    #region Can Execute Methods
    private bool CanSaveSettings() => !IsBusy;

    private bool CanSaveUserInfo()
    {
        return !IsBusy && 
               !string.IsNullOrWhiteSpace(Nom) && 
               !string.IsNullOrWhiteSpace(Prenom) && 
               !string.IsNullOrWhiteSpace(Email) &&
               Email.Contains("@");
    }

    private bool CanChangePassword()
    {
        return !IsBusy && 
               !string.IsNullOrWhiteSpace(CurrentPassword) && 
               !string.IsNullOrWhiteSpace(NewPassword) && 
               NewPassword.Length >= 6 &&
               NewPassword == ConfirmPassword;
    }

    private void RefreshCanExecute()
    {
        ((Command)SaveSettingsCommand)?.ChangeCanExecute();
        ((Command)SaveUserInfoCommand)?.ChangeCanExecute();
        ((Command)ChangePasswordCommand)?.ChangeCanExecute();
    }
    #endregion

    #region INotifyPropertyChanged
    public event PropertyChangedEventHandler PropertyChanged;

    protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected bool SetProperty<T>(ref T backingStore, T value, [CallerMemberName] string propertyName = "")
    {
        if (EqualityComparer<T>.Default.Equals(backingStore, value))
            return false;

        backingStore = value;
        OnPropertyChanged(propertyName);
        return true;
    }
    #endregion
}