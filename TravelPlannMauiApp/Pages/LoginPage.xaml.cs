using TravelPlannMauiApp.ViewModels;

namespace TravelPlannMauiApp.Pages;

public partial class LoginPage : ContentPage
{
    private readonly LoginViewModel _viewModel;

    public LoginPage(LoginViewModel viewModel)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = _viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        
        // Focus automatique sur le champ email si vide
        if (string.IsNullOrEmpty(_viewModel.Email))
        {
            EmailEntry.Focus();
        }
    }

    // Gestion de l'événement Enter pour passer au champ suivant
    private void OnEmailCompleted(object sender, EventArgs e)
    {
        PasswordEntry.Focus();
    }

    private void OnPasswordCompleted(object sender, EventArgs e)
    {
        if (_viewModel.LoginCommand.CanExecute(null))
        {
            _viewModel.LoginCommand.Execute(null);
        }
    }
}