namespace BU.Services;

public interface ISessionService
{
    Task SetCurrentUserAsync(int userId, string userName);
    Task<int?> GetCurrentUserIdAsync();
    Task<string?> GetCurrentUserNameAsync();
    Task ClearSessionAsync();
    bool IsLoggedIn();
}