using TravelPlannMauiApp.ViewModels;
using Microsoft.Extensions.DependencyInjection;

namespace TravelPlannMauiApp.Pages
{
    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public SettingsPage()
        {
            InitializeComponent();
            
            try
            {
                // Obtenir le service provider de manière plus robuste
                var serviceProvider = Handler?.MauiContext?.Services ?? 
                                    (Application.Current as App)?.Handler?.MauiContext?.Services;
                
                if (serviceProvider != null)
                {
                    _viewModel = serviceProvider.GetService<SettingsViewModel>();
                }
                
                if (_viewModel == null)
                {
                    System.Diagnostics.Debug.WriteLine("ATTENTION: SettingsViewModel non injecté, tentative de création manuelle");
                    
                    // Tenter de créer manuellement avec les services disponibles
                    if (serviceProvider != null)
                    {
                        var settingsService = serviceProvider.GetService<BU.Services.ISettingsService>();
                        var sessionService = serviceProvider.GetService<BU.Services.ISessionService>();
                        var utilisateurService = serviceProvider.GetService<BU.Services.IUtilisateurService>();
                        
                        if (settingsService != null && sessionService != null && utilisateurService != null)
                        {
                            _viewModel = new SettingsViewModel(settingsService, sessionService, utilisateurService);
                        }
                    }
                }
                
                if (_viewModel == null)
                {
                    DisplayAlert("Erreur", "Impossible de charger les paramètres. Services non disponibles.", "OK");
                    return;
                }
                
                BindingContext = _viewModel;
                
                // S'abonner aux événements du ViewModel pour la navigation
                _viewModel.NavigationRequested += OnNavigationRequested;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur initialisation SettingsPage: {ex}");
                DisplayAlert("Erreur", "Erreur lors de l'initialisation de la page des paramètres", "OK");
            }
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            try
            {
                System.Diagnostics.Debug.WriteLine("SettingsPage: OnAppearing");
                
                if (_viewModel != null)
                {
                    // Charger les paramètres et informations utilisateur
                    if (_viewModel.LoadSettingsCommand?.CanExecute(null) == true)
                    {
                        _viewModel.LoadSettingsCommand.Execute(null);
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur OnAppearing SettingsPage: {ex}");
                await DisplayAlert("Erreur", "Erreur lors du chargement des paramètres", "OK");
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            
            // Se désabonner des événements pour éviter les fuites mémoire
            if (_viewModel != null)
            {
                _viewModel.NavigationRequested -= OnNavigationRequested;
            }
        }

        private async void OnNavigationRequested(object sender, string route)
        {
            try
            {
                System.Diagnostics.Debug.WriteLine($"Navigation demandée vers: {route}");
                
                if (route == "//LoginPage")
                {
                    // Navigation vers la page de connexion (déconnexion)
                    await Shell.Current.GoToAsync(route);
                }
                else
                {
                    // Autres navigations
                    await Shell.Current.GoToAsync(route);
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur navigation: {ex}");
                await DisplayAlert("Erreur", "Erreur lors de la navigation", "OK");
            }
        }

        // Gestion des événements de changement de picker pour appliquer immédiatement certains changements
        private void OnThemePickerSelectedIndexChanged(object sender, EventArgs e)
        {
            try
            {
                var picker = sender as Picker;
                if (picker?.SelectedItem != null && _viewModel != null)
                {
                    var selectedTheme = picker.SelectedItem.ToString();
                    
                    // Appliquer le thème immédiatement
                    switch (selectedTheme)
                    {
                        case "Light":
                            Application.Current.UserAppTheme = AppTheme.Light;
                            break;
                        case "Dark":
                            Application.Current.UserAppTheme = AppTheme.Dark;
                            break;
                        case "System":
                            Application.Current.UserAppTheme = AppTheme.Unspecified;
                            break;
                    }
                    
                    System.Diagnostics.Debug.WriteLine($"Thème appliqué: {selectedTheme}");
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur changement de thème: {ex}");
            }
        }

        // Validation en temps réel des champs
        private void OnPasswordEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                // Validation visuelle des mots de passe
                var newPasswordEntry = NewPasswordEntry;
                var confirmPasswordEntry = ConfirmPasswordEntry;
                
                if (newPasswordEntry != null && confirmPasswordEntry != null)
                {
                    var newPassword = newPasswordEntry.Text ?? "";
                    var confirmPassword = confirmPasswordEntry.Text ?? "";
                    
                    // Changer la couleur de fond selon la validation
                    if (!string.IsNullOrEmpty(newPassword) && !string.IsNullOrEmpty(confirmPassword))
                    {
                        if (newPassword == confirmPassword && newPassword.Length >= 6)
                        {
                            confirmPasswordEntry.BackgroundColor = Color.FromArgb("#E8F5E8"); // Vert clair
                        }
                        else
                        {
                            confirmPasswordEntry.BackgroundColor = Color.FromArgb("#FFF0F0"); // Rouge clair
                        }
                    }
                    else
                    {
                        confirmPasswordEntry.BackgroundColor = Color.FromArgb("#F8F8F8"); // Neutre
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur validation mot de passe: {ex}");
            }
        }

        // Validation de l'email en temps réel
        private void OnEmailEntryTextChanged(object sender, TextChangedEventArgs e)
        {
            try
            {
                var emailEntry = sender as Entry;
                if (emailEntry != null)
                {
                    var email = emailEntry.Text ?? "";
                    
                    // Validation simple de l'email
                    if (!string.IsNullOrEmpty(email))
                    {
                        if (email.Contains("@") && email.Contains("."))
                        {
                            emailEntry.BackgroundColor = Color.FromArgb("#E8F5E8"); // Vert clair
                        }
                        else
                        {
                            emailEntry.BackgroundColor = Color.FromArgb("#FFF0F0"); // Rouge clair
                        }
                    }
                    else
                    {
                        emailEntry.BackgroundColor = Color.FromArgb("#F8F8F8"); // Neutre
                    }
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur validation email: {ex}");
            }
        }
    }
}