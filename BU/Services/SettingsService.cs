namespace BU.Services;

public class SettingsService : ISettingsService
{
    private const string SETTINGS_KEY = "app_settings";

    public async Task<SettingsModel> GetSettingsAsync()
    {
        try
        {
            var settingsJson = await SecureStorage.GetAsync(SETTINGS_KEY);
            if (!string.IsNullOrEmpty(settingsJson))
            {
                return JsonSerializer.Deserialize<SettingsModel>(settingsJson) ?? new SettingsModel();
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors du chargement des paramètres: {ex.Message}");
        }

        return new SettingsModel();
    }

    public async Task SaveSettingsAsync(SettingsModel settings)
    {
        try
        {
            var settingsJson = JsonSerializer.Serialize(settings);
            await SecureStorage.SetAsync(SETTINGS_KEY, settingsJson);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur lors de la sauvegarde des paramètres: {ex.Message}");
            throw;
        }
    }
}