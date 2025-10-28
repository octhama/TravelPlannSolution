using BU.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TravelPlannMauiApp.ViewModels
{
    public class LeaderboardViewModel : BaseViewModel
    {
        private readonly IClassementVoyageurService? _classementService;
        private readonly ISessionService? _sessionService;

        private ObservableCollection<DAL.DB.ClassementVoyageur> _classement = new();
        private DAL.DB.ClassementVoyageur? _currentUserRanking;

        public LeaderboardViewModel()
        {
            // Default constructor for XAML design time
        }

        public LeaderboardViewModel(IClassementVoyageurService classementService,
                                  ISessionService sessionService)
        {
            _classementService = classementService;
            _sessionService = sessionService;

            LoadClassementCommand = new Command(async () => await LoadClassementAsync());
            UpdateClassementCommand = new Command(async () => await UpdateClassementAsync());

            _ = LoadClassementAsync();
        }

        #region Properties

        public ObservableCollection<DAL.DB.ClassementVoyageur> Classement
        {
            get => _classement;
            set => SetProperty(ref _classement, value);
        }

        public DAL.DB.ClassementVoyageur? CurrentUserRanking
        {
            get => _currentUserRanking;
            set => SetProperty(ref _currentUserRanking, value);
        }

        #endregion

        #region Commands

        public ICommand? LoadClassementCommand { get; }
        public ICommand? UpdateClassementCommand { get; }

        #endregion

        #region Methods

        private async Task LoadClassementAsync()
        {
            if (_classementService == null || _sessionService == null) return;

            try
            {
                IsBusy = true;

                // Charger le classement général
                var classement = await _classementService.GetClassementAsync();
                Classement = new ObservableCollection<DAL.DB.ClassementVoyageur>(classement.Take(10)); // Top 10

                // Charger le classement de l'utilisateur actuel
                var userId = await _sessionService.GetCurrentUserIdAsync();
                if (userId.HasValue)
                {
                    CurrentUserRanking = await _classementService.GetClassementByUtilisateurAsync(userId.Value);
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erreur", $"Erreur lors du chargement du classement: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task UpdateClassementAsync()
        {
            if (_classementService == null) return;

            try
            {
                IsBusy = true;

                var success = await _classementService.UpdateClassementAsync();
                if (success)
                {
                    await LoadClassementAsync();
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Succès", "Classement mis à jour", "OK");
                }
                else
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Erreur", "Erreur lors de la mise à jour du classement", "OK");
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erreur", $"Erreur: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion
    }
}