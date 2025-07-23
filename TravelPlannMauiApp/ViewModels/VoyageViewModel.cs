using BU.Services;
using DAL.DB;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using TravelPlannMauiApp.Pages;
using System.Diagnostics;

namespace TravelPlannMauiApp.ViewModels
{
    public class VoyageViewModel : BaseViewModel
    {
        private readonly IVoyageService _voyageService;
        private bool _isBusy;
        public ObservableCollection<Voyage> Voyages { get; } = new();
        public ICommand LoadVoyagesCommand { get; }
        public ICommand ViewVoyageDetailsCommand { get; }
        public ICommand AddVoyageCommand { get; }
        public ICommand ToggleCompleteVoyageCommand { get; }
        public ICommand ToggleArchiveVoyageCommand { get; }
        
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }
        
        public VoyageViewModel(IVoyageService voyageService)
        {
            _voyageService = voyageService;

            LoadVoyagesCommand = new Command(async () => await LoadVoyagesAsync());
            ViewVoyageDetailsCommand = new Command<Voyage>(async (v) => await ViewVoyageDetails(v));
            AddVoyageCommand = new Command(async () => await AddVoyageAsync());
            ToggleCompleteVoyageCommand = new Command<Voyage>(async (v) => await ToggleCompleteVoyage(v));
            ToggleArchiveVoyageCommand = new Command<Voyage>(async (v) => await ToggleArchiveVoyage(v));
        }
     
        public async Task LoadVoyagesAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                Voyages.Clear();
                // S'assurer que les relations (Activités et Hébergements) sont chargées
                var voyages = await _voyageService.GetVoyagesAsync();
                
                // Trier les voyages par date de création décroissante (du plus récent au plus ancien)
                // Si pas de date de création, trier par VoyageId décroissant (supposé être auto-incrémenté)
                var voyagesTries = voyages.OrderByDescending(v => v.VoyageId).ToList();
                
                foreach (var v in voyagesTries)
                {
                    Voyages.Add(v);
                }
            }
            catch (Exception ex)
            {
                await HandleError(ex, "Erreur lors du chargement des voyages");
            }
            finally
            {
                IsBusy = false;
            }
        }
      
        private async Task ToggleCompleteVoyage(Voyage voyage)
        {
            if (voyage == null) return;

            try
            {
                // Récupérer le voyage complet avec ses relations depuis la DB
                var voyageComplet = await _voyageService.GetVoyageByIdAsync(voyage.VoyageId);
                if (voyageComplet == null) return;

                voyageComplet.EstComplete = !voyageComplet.EstComplete;
                if (voyageComplet.EstComplete) voyageComplet.EstArchive = false;

                await _voyageService.UpdateVoyageAsync(voyageComplet);
                
                // Mettre à jour l'objet dans la collection locale
                var index = Voyages.IndexOf(voyage);
                if (index >= 0)
                {
                    Voyages[index] = voyageComplet;
                }
                
                // Optionnel : Recharger uniquement si nécessaire
                // await LoadVoyagesAsync();
            }
            catch (Exception ex)
            {
                await HandleError(ex, "Erreur lors de la modification du statut");
            }
        }
       
        private async Task ToggleArchiveVoyage(Voyage voyage)
        {
            if (voyage == null) return;

            try
            {
                if (!voyage.EstArchive && !voyage.EstComplete)
                {
                    bool confirm = await Shell.Current.DisplayAlert(
                        "Confirmation",
                        "Archiver ce voyage non complété ?",
                        "Oui", "Non");
                    if (!confirm) return;
                }

                // Récupérer le voyage complet avec ses relations depuis la DB
                var voyageComplet = await _voyageService.GetVoyageByIdAsync(voyage.VoyageId);
                if (voyageComplet == null) return;

                voyageComplet.EstArchive = !voyageComplet.EstArchive;
                await _voyageService.UpdateVoyageAsync(voyageComplet);
                
                // Mettre à jour l'objet dans la collection locale
                var index = Voyages.IndexOf(voyage);
                if (index >= 0)
                {
                    Voyages[index] = voyageComplet;
                }
                
                // Optionnel : Recharger uniquement si nécessaire
                // await LoadVoyagesAsync();
            }
            catch (Exception ex)
            {
                await HandleError(ex, "Erreur lors de l'archivage");
            }
        }
        
        public async Task ViewVoyageDetails(Voyage voyage)
        {
            if (voyage == null) return;

            try
            {
                Debug.WriteLine($"Navigation vers détails du voyage ID: {voyage.VoyageId}");
                
                // Récupérer le voyage complet avec toutes ses relations
                var voyageComplet = await _voyageService.GetVoyageByIdAsync(voyage.VoyageId);
                if (voyageComplet == null)
                {
                    await Shell.Current.DisplayAlert("Erreur", "Voyage introuvable", "OK");
                    return;
                }
                
                var dto = new VoyageDetailsDTO
                {
                    VoyageID = voyageComplet.VoyageId,
                    NomVoyage = voyageComplet.NomVoyage,
                    Description = voyageComplet.Description,
                    DateDebut = voyageComplet.DateDebut.ToDateTime(TimeOnly.MinValue),
                    DateFin = voyageComplet.DateFin.ToDateTime(TimeOnly.MinValue),
                    EstComplete = voyageComplet.EstComplete,
                    EstArchive = voyageComplet.EstArchive,
                    // S'assurer que les relations sont incluses
                    NombreActivites = voyageComplet.Activites?.Count ?? 0,
                    NombreHebergements = voyageComplet.Hebergements?.Count ?? 0
                };

                var serialized = JsonSerializer.Serialize(dto);
                Debug.WriteLine($"Données sérialisées: {serialized}");

                await Shell.Current.GoToAsync($"{nameof(VoyageDetailsPage)}?ViewModel={Uri.EscapeDataString(serialized)}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur navigation: {ex}");
                await HandleError(ex, "Erreur lors de la navigation");
            }
        }

        public async Task RefreshVoyagesAsync()
        {
            if (IsBusy) return;

            IsBusy = true;
            try
            {
                // S'assurer que les relations sont chargées
                var voyages = await _voyageService.GetVoyagesAsync();
                
                // Trier les voyages par VoyageId décroissant (du plus récent au plus ancien)
                var voyagesTries = voyages.OrderByDescending(v => v.VoyageId).ToList();

                Device.BeginInvokeOnMainThread(() =>
                {
                    Voyages.Clear();
                    foreach (var v in voyagesTries)
                    {
                        Voyages.Add(v);
                    }
                });
            }
            catch (Exception ex)
            {
                await HandleError(ex, "Erreur lors du chargement des voyages");
            }
            finally
            {
                IsBusy = false;
            }
        }
        
        public async Task AddVoyageAsync()
        {
            await Shell.Current.GoToAsync(nameof(AddVoyagePage));
        }
    }
    
    public class VoyageDetailsDTO
    {
        public int VoyageID { get; set; }
        public string NomVoyage { get; set; }
        public string Description { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public bool EstComplete { get; set; }
        public bool EstArchive { get; set; }
        public int NombreActivites { get; set; }
        public int NombreHebergements { get; set; }
    }
}