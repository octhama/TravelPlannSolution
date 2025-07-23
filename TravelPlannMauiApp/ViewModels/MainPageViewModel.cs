using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using System.Diagnostics;
using BU.Models;
using BU.Services;
using System.Linq;
using DAL.DB;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace TravelPlannMauiApp.ViewModels
{
    // ViewModels/MainPageViewModel.cs
    public class MainPageViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly IVoyageService _voyageService;

        private BU.Models.Utilisateur _utilisateur;
        public BU.Models.Utilisateur Utilisateur
        {
            get => _utilisateur;
            set => SetProperty(ref _utilisateur, value);
        }

        private Voyage _prochainVoyage;
        public Voyage ProchainVoyage
        {
            get => _prochainVoyage;
            set => SetProperty(ref _prochainVoyage, value);
        }

        public ICommand DeconnexionCommand { get; }

        public MainPageViewModel(IAuthService authService, IVoyageService voyageService)
        {
            _authService = authService;
            _voyageService = voyageService;

            DeconnexionCommand = new Command(async () => await Deconnexion());

            ChargerDonnees();
        }

        private async void ChargerDonnees()
        {
            IsBusy = true;

            Utilisateur = await _authService.GetUtilisateurActuel();

            if (Utilisateur != null)
            {
                var voyages = await _voyageService.GetVoyagesParUtilisateur(Utilisateur.UtilisateurID);
                ProchainVoyage = voyages.OrderBy(v => v.DateDebut).FirstOrDefault();
            }

            IsBusy = false;
        }

        private async Task Deconnexion()
        {
            await _authService.Deconnecter();
            await Shell.Current.GoToAsync("//ConnexionPage");
        }
    }
}