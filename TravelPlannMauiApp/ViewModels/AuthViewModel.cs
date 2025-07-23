
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
    // ViewModels/AuthViewModel.cs
    public class AuthViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly INavigationService _navigationService;

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _motDePasse;
        public string MotDePasse
        {
            get => _motDePasse;
            set => SetProperty(ref _motDePasse, value);
        }

        public ICommand ConnexionCommand { get; }
        public ICommand InscriptionCommand { get; }

        public AuthViewModel(IAuthService authService, INavigationService navigationService)
        {
            _authService = authService;
            _navigationService = navigationService;

            ConnexionCommand = new Command(async () => await Connexion());
            InscriptionCommand = new Command(async () => await Inscription());
        }

        private async Task Connexion()
        {
            if (string.IsNullOrWhiteSpace(Email) return;
            if (string.IsNullOrWhiteSpace(MotDePasse)) return;

            IsBusy = true;

            var succes = await _authService.Authentifier(Email, MotDePasse);
            if (succes)
            {
                await _navigationService.NavigateToAsync("//MainPage");
            }
            else
            {
                await Application.Current.MainPage.DisplayAlert("Erreur", "Email ou mot de passe incorrect", "OK");
            }

            IsBusy = false;
        }

        private async Task Inscription()
        {
            await _navigationService.NavigateToAsync(nameof(InscriptionPage));
        }
    }
}