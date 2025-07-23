using System.Windows.Input;
using BU.Services;

namespace TravelPlannMauiApp.ViewModels;

public class SettingsViewModel
{
    private readonly ISettingsService _settingsService;
    private readonly INavigationService _navigationService;

    public SettingsViewModel(ISettingsService settingsService, INavigationService navigationService)
    {
        _settingsService = settingsService;
        _navigationService = navigationService;
        LoadSettingsCommand = new Command(LoadSettings);
        SaveSettingsCommand = new Command(SaveSettings);
    }

    public ICommand LoadSettingsCommand { get; }
    public ICommand SaveSettingsCommand { get; }

    public string Language { get; set; }
    public string Theme { get; set; }

    private void LoadSettings()
    {
        
    }

    private void SaveSettings()
    {
      
    }
    
}
