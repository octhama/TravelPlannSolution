using BU.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TravelPlannMauiApp.ViewModels
{
    public class RewardsViewModel : BaseViewModel
    {
        private readonly IPointsRecompenseService? _pointsService;
        private readonly INiveauRecompenseService? _niveauService;
        private readonly ISessionService? _sessionService;

        private ObservableCollection<DAL.DB.PointsRecompense> _pointsHistory = new();
        private ObservableCollection<DAL.DB.NiveauRecompense> _niveaux = new();
        private DAL.DB.NiveauRecompense? _currentNiveau;
        private int _totalPoints;
        private int _pointsToNextLevel;

        public RewardsViewModel()
        {
            // Default constructor for XAML design time
        }

        public RewardsViewModel(IPointsRecompenseService pointsService,
                              INiveauRecompenseService niveauService,
                              ISessionService sessionService)
        {
            _pointsService = pointsService;
            _niveauService = niveauService;
            _sessionService = sessionService;

            LoadDataCommand = new Command(async () => await LoadDataAsync());

            _ = LoadDataAsync();
        }

        #region Properties

        public ObservableCollection<DAL.DB.PointsRecompense> PointsHistory
        {
            get => _pointsHistory;
            set => SetProperty(ref _pointsHistory, value);
        }

        public ObservableCollection<DAL.DB.NiveauRecompense> Niveaux
        {
            get => _niveaux;
            set => SetProperty(ref _niveaux, value);
        }

        public DAL.DB.NiveauRecompense? CurrentNiveau
        {
            get => _currentNiveau;
            set => SetProperty(ref _currentNiveau, value);
        }

        public int TotalPoints
        {
            get => _totalPoints;
            set => SetProperty(ref _totalPoints, value);
        }

        public int PointsToNextLevel
        {
            get => _pointsToNextLevel;
            set => SetProperty(ref _pointsToNextLevel, value);
        }

        public double ProgressToNextLevel => CurrentNiveau != null && PointsToNextLevel > 0
            ? (double)(TotalPoints - CurrentNiveau.PointsRequis) / (PointsToNextLevel) : 0;

        #endregion

        #region Commands

        public ICommand? LoadDataCommand { get; }

        #endregion

        #region Methods

        private async Task LoadDataAsync()
        {
            if (_sessionService == null || _pointsService == null || _niveauService == null) return;

            try
            {
                IsBusy = true;

                var userId = await _sessionService.GetCurrentUserIdAsync();
                if (!userId.HasValue) return;

                // Charger l'historique des points
                var pointsHistory = await _pointsService.GetPointsHistoryAsync(userId.Value);
                PointsHistory = new ObservableCollection<DAL.DB.PointsRecompense>(pointsHistory);

                // Charger le total des points
                TotalPoints = await _pointsService.GetTotalPointsAsync(userId.Value);

                // Charger tous les niveaux
                var niveaux = await _niveauService.GetNiveauxAsync();
                Niveaux = new ObservableCollection<DAL.DB.NiveauRecompense>(niveaux);

                // Déterminer le niveau actuel
                CurrentNiveau = await _niveauService.GetNiveauByPointsAsync(TotalPoints);

                // Calculer les points pour le prochain niveau
                if (CurrentNiveau != null)
                {
                    var nextLevel = niveaux.FirstOrDefault(n => n.PointsRequis > CurrentNiveau.PointsRequis);
                    PointsToNextLevel = nextLevel != null ? nextLevel.PointsRequis - CurrentNiveau.PointsRequis : 0;
                }
                else
                {
                    PointsToNextLevel = 0;
                }

                OnPropertyChanged(nameof(ProgressToNextLevel));
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erreur", $"Erreur lors du chargement: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        #endregion
    }
}