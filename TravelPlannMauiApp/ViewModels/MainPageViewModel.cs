namespace TravelPlannMauiApp.ViewModels;

public class MainPageViewModel : BaseViewModel
{
    private string _userName = "Utilisateur";

    public MainPageViewModel()
    {
        LoadUserInfoAsync();
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    public async Task LoadUserInfoAsync()
    {
        try
        {
            // Utiliser les méthodes de fallback du LoginViewModel
            var userName = await LoginViewModel.GetCurrentUserNameAsync();
            
            if (!string.IsNullOrEmpty(userName))
            {
                UserName = userName;
            }
            else
            {
                UserName = "Utilisateur";
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des infos utilisateur: {ex}");
            UserName = "Utilisateur";
        }
    }
}