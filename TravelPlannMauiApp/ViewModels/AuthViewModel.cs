using BU.Models;
using BU.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Microsoft.Extensions.Logging;

namespace TravelPlannMauiApp.ViewModels
{
    public class AuthViewModel : BaseViewModel
    {
        private readonly IAuthService _authService;
        private readonly ILogger<AuthViewModel> _logger;

        private string _email = string.Empty;
        public string Email
        {
            get => _email;
            set
            {
                SetProperty(ref _email, value);
                OnPropertyChanged(nameof(PeutSeConnecter));
            }
        }

        private string _motDePasse = string.Empty;
        public string MotDePasse
        {
            get => _motDePasse;
            set
            {
                SetProperty(ref _motDePasse, value);
                OnPropertyChanged(nameof(PeutSeConnecter));
            }
        }

        private string _nom = string.Empty;
        public string Nom
        {
            get => _nom;
            set => SetProperty(ref _nom, value);
        }

        private string _prenom = string.Empty;
        public string Prenom
        {
            get => _prenom;
            set => SetProperty(ref _prenom, value);
        }

        private string _confirmationMotDePasse = string.Empty;
        public string ConfirmationMotDePasse
        {
            get => _confirmationMotDePasse;
            set
            {
                SetProperty(ref _confirmationMotDePasse, value);
                OnPropertyChanged(nameof(PeutSInscrire));
            }
        }

        private string _messageErreur = string.Empty;
        public string MessageErreur
        {
            get => _messageErreur;
            set => SetProperty(ref _messageErreur, value);
        }

        public bool PeutSeConnecter => !string.IsNullOrWhiteSpace(Email) && 
                                      !string.IsNullOrWhiteSpace(MotDePasse) && 
                                      !IsBusy;

        public bool PeutSInscrire => !string.IsNullOrWhiteSpace(Email) && 
                                    !string.IsNullOrWhiteSpace(MotDePasse) && 
                                    !string.IsNullOrWhiteSpace(Nom) && 
                                    !string.IsNullOrWhiteSpace(Prenom) &&
                                    MotDePasse == ConfirmationMotDePasse && 
                                    !IsBusy;

        public ICommand ConnexionCommand { get; }
        public ICommand InscriptionCommand { get; }
        public ICommand NaviguerVersInscriptionCommand { get; }
        public ICommand NaviguerVersConnexionCommand { get; }

        public AuthViewModel(IAuthService authService, ILogger<AuthViewModel> logger)
        {
            _authService = authService ?? throw new ArgumentNullException(nameof(authService));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            ConnexionCommand = new Command(async () => await ConnexionAsync(), () => PeutSeConnecter);
            InscriptionCommand = new Command(async () => await InscriptionAsync(), () => PeutSInscrire);
            NaviguerVersInscriptionCommand = new Command(async () => await NaviguerVersInscriptionAsync());
            NaviguerVersConnexionCommand = new Command(async () => await NaviguerVersConnexionAsync());
        }

        private async Task ConnexionAsync()
        {
            try
            {
                IsBusy = true;
                MessageErreur = string.Empty;

                var resultat = await _authService.AuthentiquerAsync(Email, MotDePasse);
                
                if (resultat.Succes)
                {
                    await Shell.Current.GoToAsync("//MainPage");
                }
                else
                {
                    MessageErreur = resultat.MessageErreur;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de la connexion");
                MessageErreur = "Une erreur inattendue s'est produite";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task InscriptionAsync()
        {
            try
            {
                IsBusy = true;
                MessageErreur = string.Empty;

                var nouvelUtilisateur = new Utilisateur
                {
                    Email = Email,
                    MotDePasseHash = MotDePasse, // Sera hashé dans le service
                    Nom = Nom,
                    Prenom = Prenom
                };

                var resultat = await _authService.EnregistrerAsync(nouvelUtilisateur);
                
                if (resultat.Succes)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Succès", 
                        "Compte créé avec succès !", 
                        "OK");
                    
                    await Shell.Current.GoToAsync("//ConnexionPage");
                }
                else
                {
                    MessageErreur = resultat.MessageErreur;
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erreur lors de l'inscription");
                MessageErreur = "Une erreur inattendue s'est produite";
            }
            finally
            {
                IsBusy = false;
            }
        }

        private async Task NaviguerVersInscriptionAsync()
        {
            await Shell.Current.GoToAsync("//InscriptionPage");
        }

        private async Task NaviguerVersConnexionAsync()
        {
            await Shell.Current.GoToAsync("//ConnexionPage");
        }

        public void ReinitialiserFormulaire()
        {
            Email = string.Empty;
            MotDePasse = string.Empty;
            Nom = string.Empty;
            Prenom = string.Empty;
            ConfirmationMotDePasse = string.Empty;
            MessageErreur = string.Empty;
        }
    }
}