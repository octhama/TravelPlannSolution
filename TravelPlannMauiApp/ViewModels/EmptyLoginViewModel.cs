using System.Windows.Input;


namespace TravelPlannMauiApp.ViewModels;
// ViewModel de secours pour éviter les crashs (en cas d'absence de service)
public class EmptyLoginViewModel : BaseViewModel
{
    private string _email = "";
    private string _motDePasse = "";

    public string Email
    {
        get => _email;
        set => SetProperty(ref _email, value);
    }

    public string MotDePasse
    {
        get => _motDePasse;
        set => SetProperty(ref _motDePasse, value);
    }

    public ICommand LoginCommand { get; }
    public ICommand RegisterCommand { get; }

    public EmptyLoginViewModel()
    {
        LoginCommand = CreateCommand(async () =>
        {
            await Shell.Current.DisplayAlert("Erreur", "Service non disponible. Redémarrez l'application.", "OK");
        });

        RegisterCommand = CreateCommand(async () =>
        {
            await Shell.Current.DisplayAlert("Erreur", "Service non disponible. Redémarrez l'application.", "OK");
        });
    }
}