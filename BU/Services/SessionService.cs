namespace BU.Services;

public class SessionService : ISessionService
{
    private int? _currentUserId;
    private string? _currentUserName;

    public async Task SetCurrentUserAsync(int userId, string userName)
    {
        _currentUserId = userId;
        _currentUserName = userName;

        // Essayer de sauvegarder dans SecureStorage, sinon utiliser Preferences
        try
        {
            await SecureStorage.SetAsync("current_user_id", userId.ToString());
            await SecureStorage.SetAsync("current_user_name", userName);
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SecureStorage failed, using Preferences: {ex.Message}");
            Preferences.Set("current_user_id", userId.ToString());
            Preferences.Set("current_user_name", userName);
        }
    }

    public async Task<int?> GetCurrentUserIdAsync()
    {
        if (_currentUserId.HasValue)
            return _currentUserId;

        try
        {
            var userIdString = await SecureStorage.GetAsync("current_user_id");
            if (int.TryParse(userIdString, out int userId))
            {
                _currentUserId = userId;
                return userId;
            }
        }
        catch (Exception)
        {
            // Fallback vers Preferences
            var userIdString = Preferences.Get("current_user_id", null);
            if (int.TryParse(userIdString, out int userId))
            {
                _currentUserId = userId;
                return userId;
            }
        }

        return null;
    }

    public async Task<string?> GetCurrentUserNameAsync()
    {
        if (!string.IsNullOrEmpty(_currentUserName))
            return _currentUserName;

        try
        {
            _currentUserName = await SecureStorage.GetAsync("current_user_name");
            return _currentUserName;
        }
        catch (Exception)
        {
            // Fallback vers Preferences
            _currentUserName = Preferences.Get("current_user_name", null);
            return _currentUserName;
        }
    }

    public async Task ClearSessionAsync()
    {
        _currentUserId = null;
        _currentUserName = null;

        try
        {
            SecureStorage.RemoveAll();
        }
        catch (Exception)
        {
            Preferences.Clear();
        }
    }

    public bool IsLoggedIn()
    {
        return _currentUserId.HasValue || GetCurrentUserIdAsync().Result.HasValue;
    }
}