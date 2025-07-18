namespace BU.Services;

public interface ISettingsService
{
    object GetSettings();
    void SaveSettings(object settings);
    
}