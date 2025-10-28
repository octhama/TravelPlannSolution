using BU.Services;
using System.Collections.ObjectModel;
using System.Windows.Input;

namespace TravelPlannMauiApp.ViewModels
{
    public class GroupManagementViewModel : BaseViewModel
    {
        private readonly IGroupeVoyageService? _groupeService;
        private readonly ISessionService? _sessionService;

        private ObservableCollection<DAL.DB.GroupeVoyage> _groupes = new();
        private ObservableCollection<DAL.DB.MembreGroupe> _membres = new();
        private DAL.DB.GroupeVoyage? _selectedGroupe;
        private string _newGroupName = string.Empty;

        public GroupManagementViewModel()
        {
            // Default constructor for XAML design time
        }

        public GroupManagementViewModel(IGroupeVoyageService groupeService,
                                      ISessionService sessionService)
        {
            _groupeService = groupeService;
            _sessionService = sessionService;

            LoadDataCommand = new Command(async () => await LoadDataAsync());
            CreateGroupCommand = new Command(async () => await CreateGroupAsync());
            DeleteGroupCommand = new Command(async () => await DeleteGroupAsync());
            AddMemberCommand = new Command(async () => await AddMemberAsync());
            RemoveMemberCommand = new Command(async () => await RemoveMemberAsync());

            _ = LoadDataAsync();
        }

        #region Properties

        public ObservableCollection<DAL.DB.GroupeVoyage> Groupes
        {
            get => _groupes;
            set => SetProperty(ref _groupes, value);
        }

        public ObservableCollection<DAL.DB.MembreGroupe> Membres
        {
            get => _membres;
            set => SetProperty(ref _membres, value);
        }

        public DAL.DB.GroupeVoyage? SelectedGroupe
        {
            get => _selectedGroupe;
            set
            {
                SetProperty(ref _selectedGroupe, value);
                if (value != null)
                {
                    _ = LoadMembresAsync();
                }
            }
        }

        public string NewGroupName
        {
            get => _newGroupName;
            set => SetProperty(ref _newGroupName, value);
        }

        #endregion

        #region Commands

        public ICommand LoadDataCommand { get; }
        public ICommand CreateGroupCommand { get; }
        public ICommand DeleteGroupCommand { get; }
        public ICommand AddMemberCommand { get; }
        public ICommand RemoveMemberCommand { get; }

        #endregion

        #region Methods

        private async Task LoadDataAsync()
        {
            if (_sessionService == null || _groupeService == null) return;

            try
            {
                IsBusy = true;

                var userId = await _sessionService.GetCurrentUserIdAsync();
                if (userId == null) return;

                // Charger les groupes de l'utilisateur
                var groupes = await _groupeService.GetGroupesByUtilisateurAsync(userId.Value);
                Groupes = new ObservableCollection<DAL.DB.GroupeVoyage>(groupes);
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

        private async Task LoadMembresAsync()
        {
            if (SelectedGroupe == null) return;

            try
            {
                var membres = await _groupeService.GetMembresAsync(SelectedGroupe.GroupeId);
                Membres = new ObservableCollection<DAL.DB.MembreGroupe>(membres);
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erreur", $"Erreur lors du chargement des membres: {ex.Message}", "OK");
            }
        }

        private async Task CreateGroupAsync()
        {
            if (_groupeService == null) return;

            try
            {
                if (string.IsNullOrWhiteSpace(NewGroupName))
                {
                    if (Application.Current?.MainPage != null)
                        await Application.Current.MainPage.DisplayAlert("Erreur", "Veuillez saisir un nom de groupe", "OK");
                    return;
                }

                await _groupeService.CreateAsync(NewGroupName);
                NewGroupName = string.Empty;
                await LoadDataAsync();
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Succès", "Groupe créé avec succès", "OK");
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erreur", $"Erreur lors de la création: {ex.Message}", "OK");
            }
        }

        private async Task DeleteGroupAsync()
        {
            if (_groupeService == null) return;

            try
            {
                if (SelectedGroupe == null) return;

                if (Application.Current?.MainPage != null)
                {
                    var result = await Application.Current.MainPage.DisplayAlert("Confirmation",
                        $"Voulez-vous vraiment supprimer le groupe '{SelectedGroupe.NomGroupe}' ?", "Oui", "Non");

                    if (result)
                    {
                        await _groupeService.DeleteAsync(SelectedGroupe.GroupeId);
                        await LoadDataAsync();
                        if (Application.Current?.MainPage != null)
                            await Application.Current.MainPage.DisplayAlert("Succès", "Groupe supprimé", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                if (Application.Current?.MainPage != null)
                    await Application.Current.MainPage.DisplayAlert("Erreur", $"Erreur lors de la suppression: {ex.Message}", "OK");
            }
        }

        private async Task AddMemberAsync()
        {
            // Cette méthode nécessiterait une interface pour saisir l'ID utilisateur
            // Pour l'instant, on affiche un message
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Info", "Fonctionnalité à implémenter: interface d'ajout de membre", "OK");
        }

        private async Task RemoveMemberAsync()
        {
            // Cette méthode nécessiterait une sélection de membre
            if (Application.Current?.MainPage != null)
                await Application.Current.MainPage.DisplayAlert("Info", "Fonctionnalité à implémenter: interface de suppression de membre", "OK");
        }

        #endregion
    }
}