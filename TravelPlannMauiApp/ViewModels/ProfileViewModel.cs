using System.Collections.ObjectModel;
using System.Windows.Input;
using BU.Services;
using DAL.DB;

namespace TravelPlannMauiApp.ViewModels;

public class ProfileViewModel : BaseViewModel
{
    private readonly IUtilisateurService _utilisateurService;
    private readonly IVoyageService _voyageService;
    private Utilisateur? _currentUser;
    private string _userName = string.Empty;
    private int _totalVoyages;
    private int _pointsRecompenses;

    public ProfileViewModel(IUtilisateurService utilisateurService, IVoyageService voyageService)
    {
        _utilisateurService = utilisateurService;
        _voyageService = voyageService;
        Voyages = new ObservableCollection<Voyage>();
        LoadProfileCommand = CreateCommand(LoadProfileAsync);
        LogoutCommand = CreateCommand(LogoutAsync);
        EditProfileCommand = CreateCommand(EditProfileAsync);
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public int TotalVoyages
    {
        get => _totalVoyages;
        set => SetProperty(ref _totalVoyages, value);
    }

    public int PointsRecompenses
    {
        get => _pointsRecompenses;
        set => SetProperty(ref _pointsRecompenses, value);
    }

    public ObservableCollection<Voyage> Voyages { get; }

    public ICommand LoadProfileCommand { get; }
    public ICommand LogoutCommand { get; }
    public ICommand EditProfileCommand { get; }

    private async Task LoadProfileAsync()
    {
        try
        {
            var userIdString = await SecureStorage.GetAsync("current_user_id");
            if (int.TryParse(userIdString, out int userId))
            {
                _currentUser = await _utilisateurService.GetByIdAsync(userId);
                if (_currentUser != null)
                {
                    UserName = $"{_currentUser.Prenom} {_currentUser.Nom}";
                    PointsRecompenses = _currentUser.PointsRecompenses;
                    
                    var voyages = await _voyageService.GetVoyagesByUtilisateurAsync(userId);
                    TotalVoyages = voyages.Count;
                    
                    Voyages.Clear();
                    foreach (var voyage in voyages.Take(5)) // Derniers 5 voyages
                    {
                        Voyages.Add(voyage);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            await HandleError(ex, "Erreur lors du chargement du profil");
        }
    }

    private async Task LogoutAsync()
    {
        var result = await Shell.Current.DisplayAlert("Déconnexion", 
            "Êtes-vous sûr de vouloir vous déconnecter?", "Oui", "Non");
        
        if (result)
        {
            SecureStorage.RemoveAll();
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }

    private async Task EditProfileAsync()
    {
        await Shell.Current.GoToAsync("EditProfilePage");
    }
}