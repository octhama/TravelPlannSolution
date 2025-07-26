namespace BU.Services;

public class SettingsModel
{
    public string Language { get; set; } = "fr";
    public string Theme { get; set; } = "Light";
    public bool NotificationsEnabled { get; set; } = true;
    public bool LocationEnabled { get; set; } = true;
    public string Currency { get; set; } = "EUR";
}

public interface ISettingsService
{
    Task<SettingsModel> GetSettingsAsync();
    Task SaveSettingsAsync(SettingsModel settings);
}