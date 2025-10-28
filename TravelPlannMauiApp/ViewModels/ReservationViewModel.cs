using BU.Entities;
using BU.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;

namespace TravelPlannMauiApp.ViewModels
{
    public class ReservationViewModel : BaseViewModel
    {
        private readonly IReservationHebergementService? _reservationService;
        private readonly IVoyageService? _voyageService;
        private readonly IHebergementService? _hebergementService;
        private readonly ISessionService? _sessionService;

        private ObservableCollection<DAL.DB.ReservationHebergement> _reservations = new();
        private ObservableCollection<DAL.DB.Voyage> _voyages = new();
        private ObservableCollection<DAL.DB.Hebergement> _hebergements = new();
        private DAL.DB.ReservationHebergement? _selectedReservation;
        private DAL.DB.Voyage? _selectedVoyage;
        private DAL.DB.Hebergement? _selectedHebergement;
        private DateTime _dateDebut = DateTime.Now;
        private DateTime _dateFin = DateTime.Now.AddDays(1);
        private int _nombrePersonnes = 1;
        private decimal _prixTotal;

        public ReservationViewModel()
        {
            // Default constructor for XAML design time
        }

        public ReservationViewModel(IReservationHebergementService reservationService,
                                  IVoyageService voyageService,
                                  IHebergementService hebergementService,
                                  ISessionService sessionService)
        {
            _reservationService = reservationService;
            _voyageService = voyageService;
            _hebergementService = hebergementService;
            _sessionService = sessionService;

            LoadDataCommand = new Command(async () => await LoadDataAsync());
            CreateReservationCommand = new Command(async () => await CreateReservationAsync());
            UpdateReservationCommand = new Command(async () => await UpdateReservationAsync());
            DeleteReservationCommand = new Command(async () => await DeleteReservationAsync());
            CalculatePriceCommand = new Command(CalculatePrice);

            _ = LoadDataAsync();
        }

        #region Properties

        public ObservableCollection<DAL.DB.ReservationHebergement> Reservations
        {
            get => _reservations;
            set => SetProperty(ref _reservations, value);
        }

        public ObservableCollection<DAL.DB.Voyage> Voyages
        {
            get => _voyages;
            set => SetProperty(ref _voyages, value);
        }

        public ObservableCollection<DAL.DB.Hebergement> Hebergements
        {
            get => _hebergements;
            set => SetProperty(ref _hebergements, value);
        }

        public DAL.DB.ReservationHebergement? SelectedReservation
        {
            get => _selectedReservation;
            set => SetProperty(ref _selectedReservation, value);
        }

        public DAL.DB.Voyage? SelectedVoyage
        {
            get => _selectedVoyage;
            set => SetProperty(ref _selectedVoyage, value);
        }

        public DAL.DB.Hebergement? SelectedHebergement
        {
            get => _selectedHebergement;
            set
            {
                SetProperty(ref _selectedHebergement, value);
                CalculatePrice();
            }
        }

        public DateTime DateDebut
        {
            get => _dateDebut;
            set
            {
                SetProperty(ref _dateDebut, value);
                CalculatePrice();
            }
        }

        public DateTime DateFin
        {
            get => _dateFin;
            set
            {
                SetProperty(ref _dateFin, value);
                CalculatePrice();
            }
        }

        public int NombrePersonnes
        {
            get => _nombrePersonnes;
            set
            {
                SetProperty(ref _nombrePersonnes, value);
                CalculatePrice();
            }
        }

        public decimal PrixTotal
        {
            get => _prixTotal;
            set => SetProperty(ref _prixTotal, value);
        }

        #endregion

        #region Commands

        public ICommand? LoadDataCommand { get; }
        public ICommand? CreateReservationCommand { get; }
        public ICommand? UpdateReservationCommand { get; }
        public ICommand? DeleteReservationCommand { get; }
        public ICommand? CalculatePriceCommand { get; }

        #endregion

        #region Methods

        private async Task LoadDataAsync()
        {
            if (_sessionService == null || _reservationService == null || _voyageService == null || _hebergementService == null) return;

            try
            {
                IsBusy = true;

                var userId = await _sessionService.GetCurrentUserIdAsync();
                if (userId == null) return;

                // Charger les réservations de l'utilisateur
                var reservations = await _reservationService.GetReservationsByUtilisateurAsync(userId.Value);
                Reservations = new ObservableCollection<DAL.DB.ReservationHebergement>(reservations);

                // Charger les voyages de l'utilisateur
                var voyages = await _voyageService.GetVoyagesByUtilisateurAsync(userId.Value);
                Voyages = new ObservableCollection<DAL.DB.Voyage>(voyages);

                // Charger tous les hébergements disponibles
                var hebergements = await _hebergementService.GetAllHebergementsAsync();
                Hebergements = new ObservableCollection<DAL.DB.Hebergement>(hebergements);
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

        private async Task CreateReservationAsync()
        {
            if (_reservationService == null) return;

            try
            {
                if (SelectedHebergement == null)
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Erreur", "Veuillez sélectionner un hébergement", "OK");
                    return;
                }

                var reservation = new DAL.DB.ReservationHebergement
                {
                    HebergementId = SelectedHebergement.HebergementId,
                    NumConfirmation = $"CONF-{DateTime.Now.Ticks}",
                    StatutReservation = true
                };

                await _reservationService.CreateReservationAsync(reservation);
                await LoadDataAsync();
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Succès", "Réservation créée avec succès", "OK");
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erreur", $"Erreur lors de la création: {ex.Message}", "OK");
            }
        }

        private async Task UpdateReservationAsync()
        {
            if (_reservationService == null || SelectedReservation == null) return;

            try
            {
                if (SelectedReservation == null) return;

                // Only StatutReservation can be updated
                SelectedReservation.StatutReservation = true; // or some logic

                await _reservationService.UpdateReservationAsync(SelectedReservation);
                await LoadDataAsync();
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Succès", "Réservation mise à jour", "OK");
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erreur", $"Erreur lors de la mise à jour: {ex.Message}", "OK");
            }
        }

        private async Task DeleteReservationAsync()
        {
            if (_reservationService == null || SelectedReservation == null) return;

            try
            {
                if (SelectedReservation == null || Application.Current?.MainPage == null) return;

                var result = await Application.Current.MainPage.DisplayAlert("Confirmation",
                    "Voulez-vous vraiment supprimer cette réservation ?", "Oui", "Non");

                if (result)
                {
                    await _reservationService.DeleteReservationAsync(SelectedReservation.ReservationId);
                    await LoadDataAsync();
                    await Application.Current.MainPage.DisplayAlert("Succès", "Réservation supprimée", "OK");
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erreur", $"Erreur lors de la suppression: {ex.Message}", "OK");
            }
        }

        private void CalculatePrice()
        {
            if (SelectedHebergement != null && DateDebut < DateFin)
            {
                var days = (DateFin - DateDebut).Days;
                PrixTotal = (SelectedHebergement.Cout ?? 0) * days * NombrePersonnes;
            }
            else
            {
                PrixTotal = 0;
            }
        }

        #endregion
    }
}