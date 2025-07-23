namespace BU.Services;

public interface INavigationService
{
    Task NavigateToAsync(string route);
    void GoBack();
}