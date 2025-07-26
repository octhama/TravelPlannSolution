using System.ComponentModel;
using System.Runtime.CompilerServices;
using BU.Services;

namespace TravelPlannMauiApp.ViewModels
{
    public class MainPageViewModel : INotifyPropertyChanged
    {
        private readonly ISessionService _sessionService;
        private string _userName = "Utilisateur";

        public MainPageViewModel() : this(null)
        {
        }

        public MainPageViewModel(ISessionService sessionService = null)
        {
            _sessionService = sessionService;
            _ = LoadUserInfoAsync(); // Chargement asynchrone initial
        }

        public string UserName
        {
            get => _userName;
            set
            {
                if (_userName != value)
                {
                    _userName = value;
                    OnPropertyChanged();
                }
            }
        }

        public async Task LoadUserInfoAsync()
        {
            try
            {
                string userName = null;
                
                if (_sessionService != null)
                {
                    // Utiliser le service de session si disponible
                    userName = await _sessionService.GetCurrentUserNameAsync();
                }
                else
                {
                    // Fallback direct sur SecureStorage/Preferences
                    userName = await GetCurrentUserNameWithFallback();
                }

                if (!string.IsNullOrEmpty(userName))
                {
                    UserName = userName;
                    System.Diagnostics.Debug.WriteLine($"Nom utilisateur chargé: {userName}");
                }
                else
                {
                    System.Diagnostics.Debug.WriteLine("Aucun nom d'utilisateur trouvé");
                    UserName = "Utilisateur";
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des infos utilisateur: {ex}");
                UserName = "Utilisateur";
            }
        }

        private async Task<string> GetCurrentUserNameWithFallback()
        {
            try
            {
                // Essayer SecureStorage d'abord
                var userName = await SecureStorage.GetAsync("current_user_name");
                if (!string.IsNullOrEmpty(userName))
                {
                    return userName;
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur SecureStorage pour nom utilisateur: {ex.Message}");
            }

            try
            {
                // Fallback vers Preferences
                var userName = Preferences.Get("current_user_name", null);
                return userName;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur Preferences pour nom utilisateur: {ex.Message}");
                return null;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}