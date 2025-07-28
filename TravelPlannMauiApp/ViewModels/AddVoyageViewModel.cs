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

        // Propriétés pour les formulaires d'activités
        private bool _showActiviteForm;
        private string _nouvelleActiviteNom;
        private string _nouvelleActiviteDescription;
        private string _nouvelleActiviteLocalisation; // NOUVEAU

        // Propriétés pour les formulaires d'hébergements
        private bool _showHebergementForm;
        private string _nouvelHebergementNom;
        private string _nouvelHebergementType;
        private string _nouvelHebergementAdresse; // NOUVEAU
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

        // Propriétés pour les activités
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

        // NOUVEAU : Propriété pour la localisation des activités
        public string NouvelleActiviteLocalisation
        {
            get => _nouvelleActiviteLocalisation;
            set => SetProperty(ref _nouvelleActiviteLocalisation, value);
        }

        // Propriétés pour les hébergements
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

        // NOUVEAU : Propriété pour l'adresse des hébergements
        public string NouvelHebergementAdresse
        {
            get => _nouvelHebergementAdresse;
            set => SetProperty(ref _nouvelHebergementAdresse, value);
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
            AnnulerAjoutActiviteCommand = new Command(() =>
            {
                ResetActiviteForm();
            });
            AnnulerAjoutHebergementCommand = new Command(() =>
            {
                ResetHebergementForm();
            });
            AjouterNouvelleActiviteCommand = new Command(async () => await AjouterNouvelleActivite());
            AjouterNouvelHebergementCommand = new Command(async () => await AjouterNouvelHebergement());

            // Commandes de suppression (suppression locale uniquement)
            SupprimerActiviteCommand = new Command<Activite>(SupprimerActiviteLocale);
            SupprimerHebergementCommand = new Command<Hebergement>(SupprimerHebergementLocal);
        }

        private void ResetActiviteForm()
        {
            NouvelleActiviteNom = string.Empty;
            NouvelleActiviteDescription = string.Empty;
            NouvelleActiviteLocalisation = string.Empty; // NOUVEAU
            ShowActiviteForm = false;
        }

        private void ResetHebergementForm()
        {
            NouvelHebergementNom = string.Empty;
            NouvelHebergementType = string.Empty;
            NouvelHebergementAdresse = string.Empty; // NOUVEAU
            NouvelHebergementCout = 0;
            ShowHebergementForm = false;
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
                    Nom = NouvelleActiviteNom?.Trim(),
                    Description = NouvelleActiviteDescription?.Trim(),
                    Localisation = NouvelleActiviteLocalisation?.Trim() // NOUVEAU
                };

                // Créer l'activité d'abord
                var activiteCree = await _activiteService.AddActiviteAsync(nouvelleActivite);

                if (activiteCree != null)
                {
                    NouvellesActivites.Add(activiteCree);
                    ResetActiviteForm();
                    
                    Debug.WriteLine($"Activité ajoutée: {activiteCree.Nom} - Localisation: {activiteCree.Localisation}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'ajout d'activité: {ex}");
                await Shell.Current.DisplayAlert("Erreur",
                    $"Erreur lors de l'ajout de l'activité: {ex.Message}", "OK");
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
                    Nom = NouvelHebergementNom?.Trim(),
                    TypeHebergement = NouvelHebergementType?.Trim(),
                    Adresse = NouvelHebergementAdresse?.Trim(), // NOUVEAU
                    Cout = NouvelHebergementCout
                };

                // Créer l'hébergement d'abord
                var hebergementCree = await _hebergementService.AddHebergementAsync(nouvelHebergement);

                if (hebergementCree != null)
                {
                    NouveauxHebergements.Add(hebergementCree);
                    ResetHebergementForm();
                    
                    Debug.WriteLine($"Hébergement ajouté: {hebergementCree.Nom} - Adresse: {hebergementCree.Adresse}");
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'ajout d'hébergement: {ex}");
                await Shell.Current.DisplayAlert("Erreur",
                    $"Erreur lors de l'ajout de l'hébergement: {ex.Message}", "OK");
            }
        }

        // Suppression locale uniquement (les éléments ne sont pas encore dans la BD pour le voyage)
        private void SupprimerActiviteLocale(Activite activite)
        {
            if (activite != null)
            {
                NouvellesActivites.Remove(activite);
                Debug.WriteLine($"Activité supprimée localement: {activite.Nom}");
            }
        }

        private void SupprimerHebergementLocal(Hebergement hebergement)
        {
            if (hebergement != null)
            {
                NouveauxHebergements.Remove(hebergement);
                Debug.WriteLine($"Hébergement supprimé localement: {hebergement.Nom}");
            }
        }

        private async Task AddVoyageAsync()
        {
            // Validation
            if (string.IsNullOrWhiteSpace(NomVoyage))
            {
                await Shell.Current.DisplayAlert("Erreur", "Le nom du voyage est requis", "OK");
                return;
            }

            if (DateFin < DateDebut)
            {
                await Shell.Current.DisplayAlert("Erreur", "La date de fin doit être après la date de début", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                Debug.WriteLine("Début de l'ajout du voyage...");

                // Créer le voyage de base SANS les relations many-to-many initialement
                var voyage = new Voyage
                {
                    NomVoyage = NomVoyage?.Trim(),
                    Description = Description?.Trim(),
                    DateDebut = DateOnly.FromDateTime(DateDebut),
                    DateFin = DateOnly.FromDateTime(DateFin),
                    EstComplete = false,
                    EstArchive = false
                };

                Debug.WriteLine($"Voyage créé avec {NouvellesActivites.Count} activités et {NouveauxHebergements.Count} hébergements à associer");

                // Debug des données avec localisation/adresse
                foreach (var activite in NouvellesActivites)
                {
                    Debug.WriteLine($"Activité: {activite.Nom} - Localisation: {activite.Localisation}");
                }
                
                foreach (var hebergement in NouveauxHebergements)
                {
                    Debug.WriteLine($"Hébergement: {hebergement.Nom} - Adresse: {hebergement.Adresse}");
                }

                // Ajouter les relations only si il y en a
                if (NouvellesActivites.Count > 0 || NouveauxHebergements.Count > 0)
                {
                    // Créer des listes distinctes pour éviter les problèmes de tracking
                    var activitesIds = NouvellesActivites.Select(a => a.ActiviteId).ToList();
                    var hebergementsIds = NouveauxHebergements.Select(h => h.HebergementId).ToList();

                    // Créer de nouvelles instances avec seulement les IDs pour éviter les conflits EF
                    voyage.Activites = activitesIds.Select(id => new Activite { ActiviteId = id }).ToList();
                    voyage.Hebergements = hebergementsIds.Select(id => new Hebergement { HebergementId = id }).ToList();
                }
                
                Debug.WriteLine($"Voyage prêt à être ajouté: {voyage.NomVoyage}, Dates: {voyage.DateDebut} - {voyage.DateFin}");
                
                // Ajouter le voyage via le service
                await _voyageService.AddVoyageAsync(voyage);
                Debug.WriteLine($"Voyage ajouté avec succès: {voyage.NomVoyage}");
                
                // Réinitialiser les champs
                NomVoyage = string.Empty;
                Description = string.Empty;
                DateDebut = DateTime.Today;
                DateFin = DateTime.Today.AddDays(1);
                NouvellesActivites.Clear();
                NouveauxHebergements.Clear();
                ShowActiviteForm = false;
                ShowHebergementForm = false;
                
                Debug.WriteLine("Réinitialisation des champs et collections après ajout du voyage");
                
                // Retour à la page précédente
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur lors de l'ajout du voyage: {ex}");
                await HandleError(ex, "Erreur lors de l'ajout du voyage");
            }
            finally
            {
                IsBusy = false;
                Debug.WriteLine("Fin de l'ajout du voyage, état IsBusy réinitialisé");
            }
        }
        
        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            // Nettoyage ou sauvegarde d'état si nécessaire
            Debug.WriteLine("OnDisappearing appelé, nettoyage éventuel effectué");
        }
    }
}