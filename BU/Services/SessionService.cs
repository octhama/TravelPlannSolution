using DAL.DB;

namespace BU.Services;

public class SessionService : ISessionService
{
    private int? _currentUserId;
    private string? _currentUserName;

    public async Task SetCurrentUserAsync(int userId, string userName)
    {
        _currentUserId = userId;
        _currentUserName = userName;

        System.Diagnostics.Debug.WriteLine($"=== SAUVEGARDE SESSION ===");
        System.Diagnostics.Debug.WriteLine($"User ID: {userId}");
        System.Diagnostics.Debug.WriteLine($"User Name: {userName}");

        // Essayer de sauvegarder dans SecureStorage, sinon utiliser Preferences
        try
        {
            await SecureStorage.SetAsync("current_user_id", userId.ToString());
            await SecureStorage.SetAsync("current_user_name", userName);
            System.Diagnostics.Debug.WriteLine("Session sauvegardée dans SecureStorage");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"SecureStorage failed, using Preferences: {ex.Message}");
            try
            {
                Preferences.Set("current_user_id", userId.ToString());
                Preferences.Set("current_user_name", userName);
                System.Diagnostics.Debug.WriteLine("Session sauvegardée dans Preferences");
            }
            catch (Exception prefEx)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur Preferences: {prefEx.Message}");
                throw new InvalidOperationException("Impossible de sauvegarder la session", prefEx);
            }
        }
    }

    public async Task<int?> GetCurrentUserIdAsync()
    {
        if (_currentUserId.HasValue)
        {
            System.Diagnostics.Debug.WriteLine($"User ID depuis cache mémoire: {_currentUserId}");
            return _currentUserId;
        }

        try
        {
            var userIdString = await SecureStorage.GetAsync("current_user_id");
            if (int.TryParse(userIdString, out int userId))
            {
                _currentUserId = userId;
                System.Diagnostics.Debug.WriteLine($"User ID depuis SecureStorage: {userId}");
                return userId;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur SecureStorage GetUserId: {ex.Message}");
            // Fallback vers Preferences
            try
            {
                var userIdString = Preferences.Get("current_user_id", null);
                if (int.TryParse(userIdString, out int userId))
                {
                    _currentUserId = userId;
                    System.Diagnostics.Debug.WriteLine($"User ID depuis Preferences: {userId}");
                    return userId;
                }
            }
            catch (Exception prefEx)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur Preferences GetUserId: {prefEx.Message}");
            }
        }

        System.Diagnostics.Debug.WriteLine("Aucun User ID trouvé");
        return null;
    }

    public async Task<string?> GetCurrentUserNameAsync()
    {
        if (!string.IsNullOrEmpty(_currentUserName))
        {
            System.Diagnostics.Debug.WriteLine($"User Name depuis cache mémoire: {_currentUserName}");
            return _currentUserName;
        }

        try
        {
            _currentUserName = await SecureStorage.GetAsync("current_user_name");
            if (!string.IsNullOrEmpty(_currentUserName))
            {
                System.Diagnostics.Debug.WriteLine($"User Name depuis SecureStorage: {_currentUserName}");
                return _currentUserName;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur SecureStorage GetUserName: {ex.Message}");
            // Fallback vers Preferences
            try
            {
                _currentUserName = Preferences.Get("current_user_name", null);
                if (!string.IsNullOrEmpty(_currentUserName))
                {
                    System.Diagnostics.Debug.WriteLine($"User Name depuis Preferences: {_currentUserName}");
                    return _currentUserName;
                }
            }
            catch (Exception prefEx)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur Preferences GetUserName: {prefEx.Message}");
            }
        }

        System.Diagnostics.Debug.WriteLine("Aucun User Name trouvé");
        return null;
    }

    public async Task ClearSessionAsync()
    {
        System.Diagnostics.Debug.WriteLine("=== NETTOYAGE SESSION ===");
        
        _currentUserId = null;
        _currentUserName = null;

        try
        {
            SecureStorage.RemoveAll();
            System.Diagnostics.Debug.WriteLine("SecureStorage nettoyé");
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur nettoyage SecureStorage: {ex.Message}");
            try
            {
                Preferences.Clear();
                System.Diagnostics.Debug.WriteLine("Preferences nettoyé");
            }
            catch (Exception prefEx)
            {
                System.Diagnostics.Debug.WriteLine($"Erreur nettoyage Preferences: {prefEx.Message}");
            }
        }
    }

    public bool IsLoggedIn()
    {
        var isLoggedInMemory = _currentUserId.HasValue;
        if (isLoggedInMemory)
        {
            System.Diagnostics.Debug.WriteLine($"IsLoggedIn (mémoire): true - User ID: {_currentUserId}");
            return true;
        }

        // Vérifier de manière synchrone (attention: peut être problématique)
        try
        {
            var asyncResult = GetCurrentUserIdAsync().Result;
            var isLoggedIn = asyncResult.HasValue;
            System.Diagnostics.Debug.WriteLine($"IsLoggedIn (stockage): {isLoggedIn}");
            return isLoggedIn;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Erreur IsLoggedIn: {ex.Message}");
            return false;
        }
    }

    // Méthode helper pour obtenir un utilisateur basique
    public async Task<Utilisateur?> GetCurrentUserAsync()
    {
        var userId = await GetCurrentUserIdAsync();
        var userName = await GetCurrentUserNameAsync();
        
        if (!userId.HasValue)
            return null;
            
        return new Utilisateur 
        { 
            UtilisateurId = userId.Value, 
            Nom = userName ?? "Utilisateur"
        };
    }
}