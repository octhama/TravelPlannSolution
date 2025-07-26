using System.ComponentModel;

namespace TravelPlannMauiApp.ViewModels;

public class MainPageViewModel : BaseViewModel
{
    private string _userName = "Utilisateur";

    public MainPageViewModel()
    {
        LoadUserInfo();
    }

    public string UserName
    {
        get => _userName;
        set => SetProperty(ref _userName, value);
    }

    private async void LoadUserInfo()
    {
        try
        {
            var userName = await SecureStorage.GetAsync("current_user_name");
            if (!string.IsNullOrEmpty(userName))
            {
                UserName = userName;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des infos utilisateur: {ex}");
        }
    }

    public async Task LoadUserInfoAsync()
    {
        try
        {
            var userName = await SecureStorage.GetAsync("current_user_name");
            if (!string.IsNullOrEmpty(userName))
            {
                UserName = userName;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des infos utilisateur: {ex}");
        }
    }
}