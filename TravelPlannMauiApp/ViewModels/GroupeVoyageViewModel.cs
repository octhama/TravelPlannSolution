using System.Collections.ObjectModel;
using System.Windows.Input;
using BU.Services;
using DAL.DB;

namespace TravelPlannMauiApp.ViewModels;

public class GroupeVoyageViewModel : BaseViewModel
{
    private readonly IGroupeVoyageService _groupeService;
    private readonly IUtilisateurService _utilisateurService;
    private GroupeVoyage? _selectedGroupe;
    private string _nouveauGroupeNom = string.Empty;

    public GroupeVoyageViewModel(IGroupeVoyageService groupeService, IUtilisateurService utilisateurService)
    {
        _groupeService = groupeService;
        _utilisateurService = utilisateurService;
        
        Groupes = new ObservableCollection<GroupeVoyage>();
        MembresGroupe = new ObservableCollection<MembreGroupe>();
        
        LoadGroupesCommand = CreateCommand(LoadGroupesAsync);
        CreateGroupeCommand = CreateCommand(CreateGroupeAsync, CanCreateGroupe);
        SelectGroupeCommand = CreateCommand<GroupeVoyage>(SelectGroupeAsync);
        AddMembreCommand = CreateCommand(AddMembreAsync);
        RemoveMembreCommand = CreateCommand<MembreGroupe>(RemoveMembreAsync);
    }

    public ObservableCollection<GroupeVoyage> Groupes { get; }
    public ObservableCollection<MembreGroupe> MembresGroupe { get; }

    public GroupeVoyage? SelectedGroupe
    {
        get => _selectedGroupe;
        set => SetProperty(ref _selectedGroupe, value);
    }

    public string NouveauGroupeNom
    {
        get => _nouveauGroupeNom;
        set
        {
            SetProperty(ref _nouveauGroupeNom, value);
            ((Command)CreateGroupeCommand).ChangeCanExecute();
        }
    }

    public ICommand LoadGroupesCommand { get; }
    public ICommand CreateGroupeCommand { get; }
    public ICommand SelectGroupeCommand { get; }
    public ICommand AddMembreCommand { get; }
    public ICommand RemoveMembreCommand { get; }

    private bool CanCreateGroupe()
    {
        return !string.IsNullOrWhiteSpace(NouveauGroupeNom);
    }

    private async Task LoadGroupesAsync()
    {
        try
        {
            var userIdString = await SecureStorage.GetAsync("current_user_id");
            if (int.TryParse(userIdString, out int userId))
            {
                var groupes = await _groupeService.GetGroupesByUtilisateurAsync(userId);
                
                Groupes.Clear();
                foreach (var groupe in groupes)
                {
                    Groupes.Add(groupe);
                }
            }
        }
        catch (Exception ex)
        {
            await HandleError(ex, "Erreur lors du chargement des groupes");
        }
    }

    private async Task CreateGroupeAsync()
    {
        try
        {
            var groupe = await _groupeService.CreateAsync(NouveauGroupeNom);

            // Ajout du créateur comme administrateur
            var userIdString = await SecureStorage.GetAsync("current_user_id");
            if (int.TryParse(userIdString, out int userId))
            {
                await _groupeService.AddMembreAsync(groupe.GroupeId, userId, "Administrateur");
            }
            
            Groupes.Add(groupe);
            NouveauGroupeNom = string.Empty;
            
            await Shell.Current.DisplayAlert("Succès", "Groupe créé avec succès!", "OK");
        }
        catch (Exception ex)
        {
            await HandleError(ex, "Erreur lors de la création du groupe");
        }
    }

    private async Task SelectGroupeAsync(GroupeVoyage groupe)
    {
        try
        {
            SelectedGroupe = groupe;
            var membres = await _groupeService.GetMembresAsync(groupe.GroupeId);
            
            MembresGroupe.Clear();
            foreach (var membre in membres)
            {
                MembresGroupe.Add(membre);
            }
        }
        catch (Exception ex)
        {
            await HandleError(ex, "Erreur lors du chargement des membres");
        }
    }

    private async Task AddMembreAsync()
    {
        if (SelectedGroupe == null) return;

        try
        {
            var email = await Shell.Current.DisplayPromptAsync("Ajouter un membre", 
                "Entrez l'email de l'utilisateur à ajouter:");
            
            if (!string.IsNullOrWhiteSpace(email))
            {
                var utilisateur = await _utilisateurService.GetByEmailAsync(email);
                if (utilisateur != null)
                {
                    var membre = await _groupeService.AddMembreAsync(
                        SelectedGroupe.GroupeId, utilisateur.UtilisateurId, "Membre");

                    // Rechargement de la liste des membres
                    await SelectGroupeAsync(SelectedGroupe);
                    
                    await Shell.Current.DisplayAlert("Succès", "Membre ajouté avec succès!", "OK");
                }
                else
                {
                    await Shell.Current.DisplayAlert("Erreur", "Utilisateur non trouvé", "OK");
                }
            }
        }
        catch (Exception ex)
        {
            await HandleError(ex, "Erreur lors de l'ajout du membre");
        }
    }

    private async Task RemoveMembreAsync(MembreGroupe membre)
    {
        if (SelectedGroupe == null) return;

        try
        {
            var result = await Shell.Current.DisplayAlert("Confirmation", 
                $"Êtes-vous sûr de vouloir retirer {membre.Utilisateur.Prenom} {membre.Utilisateur.Nom} du groupe?", 
                "Oui", "Non");
            
            if (result)
            {
                await _groupeService.RemoveMembreAsync(SelectedGroupe.GroupeId, membre.UtilisateurId);
                MembresGroupe.Remove(membre);
                
                await Shell.Current.DisplayAlert("Succès", "Membre retiré avec succès!", "OK");
            }
        }
        catch (Exception ex)
        {
            await HandleError(ex, "Erreur lors de la suppression du membre");
        }
    }
}