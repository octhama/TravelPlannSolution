using BU.Services;
using DAL.DB;
using System.Collections.ObjectModel;
using System.Windows.Input;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;
using System.Diagnostics;

namespace TravelPlannMauiApp.ViewModels
{
    public class AddVoyageViewModel : BaseViewModel
    {
        private readonly IVoyageService _voyageService;
        private readonly IActiviteService _activiteService;
        private readonly IHebergementService _hebergementService;

        // Propriétés de base pour le voyage
        private string _nomVoyage;
        private string _description;
        private DateTime _dateDebut = DateTime.Today;
        private DateTime _dateFin = DateTime.Today.AddDays(1);

        // Propriétés pour les formulaires
        private bool _showActiviteForm;
        private bool _showHebergementForm;
        private string _nouvelleActiviteNom;
        private string _nouvelleActiviteDescription;
        private string _nouvelHebergementNom;
        private string _nouvelHebergementType;
        private decimal _nouvelHebergementCout;

        // Collections pour les éléments ajoutés
        public ObservableCollection<Activite> NouvellesActivites { get; } = new();
        public ObservableCollection<Hebergement> NouveauxHebergements { get; } = new();

        // Commandes
        public ICommand AddVoyageCommand { get; }
        public ICommand GoBackCommand { get; }
        public ICommand AjouterActiviteCommand { get; }
        public ICommand AjouterHebergementCommand { get; }
        public ICommand AjouterNouvelleActiviteCommand { get; }
        public ICommand AjouterNouvelHebergementCommand { get; }
        public ICommand AnnulerAjoutActiviteCommand { get; }
        public ICommand AnnulerAjoutHebergementCommand { get; }
        public ICommand SupprimerActiviteCommand { get; }
        public ICommand SupprimerHebergementCommand { get; }

        public bool ShowActiviteForm
        {
            get => _showActiviteForm;
            set => SetProperty(ref _showActiviteForm, value);
        }

        public bool ShowHebergementForm
        {
            get => _showHebergementForm;
            set => SetProperty(ref _showHebergementForm, value);
        }

        public string NomVoyage
        {
            get => _nomVoyage;
            set => SetProperty(ref _nomVoyage, value);
        }

        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public DateTime DateDebut
        {
            get => _dateDebut;
            set => SetProperty(ref _dateDebut, value);
        }

        public DateTime DateFin
        {
            get => _dateFin;
            set => SetProperty(ref _dateFin, value);
        }

        public string NouvelleActiviteNom
        {
            get => _nouvelleActiviteNom;
            set => SetProperty(ref _nouvelleActiviteNom, value);
        }

        public string NouvelleActiviteDescription
        {
            get => _nouvelleActiviteDescription;
            set => SetProperty(ref _nouvelleActiviteDescription, value);
        }

        public string NouvelHebergementNom
        {
            get => _nouvelHebergementNom;
            set => SetProperty(ref _nouvelHebergementNom, value);
        }

        public string NouvelHebergementType
        {
            get => _nouvelHebergementType;
            set => SetProperty(ref _nouvelHebergementType, value);
        }

        public decimal NouvelHebergementCout
        {
            get => _nouvelHebergementCout;
            set => SetProperty(ref _nouvelHebergementCout, value);
        }

        public AddVoyageViewModel(IVoyageService voyageService,
                                IActiviteService activiteService,
                                IHebergementService hebergementService)
        {
            _voyageService = voyageService;
            _activiteService = activiteService;
            _hebergementService = hebergementService;

            // Commandes principales
            AddVoyageCommand = new Command(async () => await AddVoyageAsync());
            GoBackCommand = new Command(async () => await Shell.Current.GoToAsync(".."));

            // Commandes pour les formulaires
            AjouterActiviteCommand = new Command(() => ShowActiviteForm = true);
            AjouterHebergementCommand = new Command(() => ShowHebergementForm = true);
            AnnulerAjoutActiviteCommand = new Command(() => ShowActiviteForm = false);
            AnnulerAjoutHebergementCommand = new Command(() => ShowHebergementForm = false);
            AjouterNouvelleActiviteCommand = new Command(async () => await AjouterNouvelleActivite());
            AjouterNouvelHebergementCommand = new Command(async () => await AjouterNouvelHebergement());
            
            // Commandes de suppression
            SupprimerActiviteCommand = new Command<Activite>(async (a) => await SupprimerActivite(a));
            SupprimerHebergementCommand = new Command<Hebergement>(async (h) => await SupprimerHebergement(h));
        }

        private async Task AjouterNouvelleActivite()
        {
            if (string.IsNullOrWhiteSpace(NouvelleActiviteNom))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le nom de l'activité est requis", "OK");
                return;
            }

            try
            {
                var nouvelleActivite = new Activite
                {
                    Nom = NouvelleActiviteNom,
                    Description = NouvelleActiviteDescription
                };

                var activiteCree = await _activiteService.AddActiviteAsync(nouvelleActivite);
                NouvellesActivites.Add(activiteCree);
                
                // Réinitialiser le formulaire
                NouvelleActiviteNom = string.Empty;
                NouvelleActiviteDescription = string.Empty;
                ShowActiviteForm = false;
            }
            catch (Exception ex)
            {
                await HandleError(ex, "Erreur lors de l'ajout de l'activité");
            }
        }

        private async Task AjouterNouvelHebergement()
        {
            if (string.IsNullOrWhiteSpace(NouvelHebergementNom))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le nom de l'hébergement est requis", "OK");
                return;
            }

            try
            {
                var nouvelHebergement = new Hebergement
                {
                    Nom = NouvelHebergementNom,
                    TypeHebergement = NouvelHebergementType,
                    Cout = NouvelHebergementCout
                };

                var hebergementCree = await _hebergementService.AddHebergementAsync(nouvelHebergement);
                NouveauxHebergements.Add(hebergementCree);
                
                // Réinitialiser le formulaire
                NouvelHebergementNom = string.Empty;
                NouvelHebergementType = string.Empty;
                NouvelHebergementCout = 0;
                ShowHebergementForm = false;
            }
            catch (Exception ex)
            {
                await HandleError(ex, "Erreur lors de l'ajout de l'hébergement");
            }
        }

        private async Task SupprimerActivite(Activite activite)
        {
            try
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    "Confirmation",
                    $"Supprimer l'activité {activite.Nom}?",
                    "Oui", "Non");

                if (confirm)
                {
                    NouvellesActivites.Remove(activite);
                    await _activiteService.DeleteActiviteAsync(activite.ActiviteId);
                }
            }
            catch (Exception ex)
            {
                await HandleError(ex, "Erreur lors de la suppression de l'activité");
            }
        }

        private async Task SupprimerHebergement(Hebergement hebergement)
        {
            try
            {
                bool confirm = await Shell.Current.DisplayAlert(
                    "Confirmation",
                    $"Supprimer l'hébergement {hebergement.Nom}?",
                    "Oui", "Non");

                if (confirm)
                {
                    NouveauxHebergements.Remove(hebergement);
                    await _hebergementService.DeleteHebergementAsync(hebergement.HebergementId);
                }
            }
            catch (Exception ex)
            {
                await HandleError(ex, "Erreur lors de la suppression de l'hébergement");
            }
        }

        private async Task AddVoyageAsync()
        {
            try
            {
                Debug.WriteLine("Début de l'ajout du voyage...");
                
                var voyage = new Voyage
                {
                    NomVoyage = NomVoyage?.Trim(),
                    Description = Description?.Trim(),
                    DateDebut = DateOnly.FromDateTime(DateDebut),
                    DateFin = DateOnly.FromDateTime(DateFin),
                    EstComplete = false,
                    EstArchive = false,
                    Activites = NouvellesActivites?.ToList() ?? new List<Activite>(),
                    Hebergements = NouveauxHebergements?.ToList() ?? new List<Hebergement>()
                };

                Debug.WriteLine($"Voyage créé avec {voyage.Activites.Count} activités et {voyage.Hebergements.Count} hébergements");

                await _voyageService.AddVoyageAsync(voyage);
                
                Debug.WriteLine("Voyage ajouté avec succès!");
                
                ResetForm();
                VoyageAdded?.Invoke(this, voyage);
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"ERREUR CRITIQUE: {ex}");
                await Shell.Current.DisplayAlert("Erreur", 
                    $"Erreur technique: {ex.Message}\nVeuillez réessayer.", "OK");
            }
        }

        private void ResetForm()
        {
            NomVoyage = string.Empty;
            Description = string.Empty;
            DateDebut = DateTime.Today;
            DateFin = DateTime.Today.AddDays(1);
            NouvellesActivites.Clear();
            NouveauxHebergements.Clear();
            ShowActiviteForm = false;
            ShowHebergementForm = false;
        }

        public event EventHandler<Voyage> VoyageAdded;
    }
}