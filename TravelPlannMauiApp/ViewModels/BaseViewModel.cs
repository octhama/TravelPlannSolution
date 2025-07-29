namespace TravelPlannMauiApp.ViewModels
{
    public abstract class BaseViewModel : INotifyPropertyChanged
    {
        private bool _isBusy; // Indique si le ViewModel est occupé (par exemple, lors du chargement de données)
        private string _title = string.Empty; // Titre du ViewModel, utilisé pour l'affichage dans l'interface utilisateur

        public event PropertyChangedEventHandler PropertyChanged; // Événement déclenché lorsque des propriétés changent dans le ViewModel
        
        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value, onChanged: () => OnPropertyChanged(nameof(IsNotBusy)));
        }
        public bool IsNotBusy => !IsBusy; // Propriété dérivée pour vérifier si le ViewModel n'est pas occupé 

        public string Title
        {
            get => _title;
            set => SetProperty(ref _title, value);
        }
        
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        
        protected bool SetProperty<T>(ref T backingStore, T value,
                                    [CallerMemberName] string propertyName = "",
                                    Action onChanged = null)
        {
            if (EqualityComparer<T>.Default.Equals(backingStore, value))
                return false;

            backingStore = value;
            onChanged?.Invoke();
            OnPropertyChanged(propertyName);
            return true;
        }

        protected bool SetField<T>(ref T field, T value, [CallerMemberName] string propertyName = null)
        {
            return SetProperty(ref field, value, propertyName);
        }
        
        protected async Task HandleError(Exception ex, string message = "Une erreur est survenue")
        {
            Debug.WriteLine($"ERREUR: {message}");
            Debug.WriteLine($"Type: {ex.GetType()}");
            Debug.WriteLine($"Message: {ex.Message}");
            Debug.WriteLine($"Stack Trace: {ex.StackTrace}");

            if (ex.InnerException != null)
            {
                Debug.WriteLine($"INNER EXCEPTION: {ex.InnerException.Message}");
                Debug.WriteLine($"INNER STACK TRACE: {ex.InnerException.StackTrace}");
            }

            try
            {
                if (Shell.Current?.CurrentPage != null)
                {
                    await Shell.Current.DisplayAlert("Erreur", $"{message}\n\nDétails techniques:\n{ex.Message}", "OK");
                }
            }
            catch (Exception displayEx)
            {
                Debug.WriteLine($"Erreur lors de l'affichage de l'alerte: {displayEx.Message}");
            }
        }

        // Version pour les commandes asynchrones
        protected ICommand CreateCommand(Func<Task> execute, Func<bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new Command(async () =>
            {
                if (IsBusy) return;

                try
                {
                    IsBusy = true;
                    await execute();
                }
                catch (Exception ex)
                {
                    await HandleError(ex);
                }
                finally
                {
                    IsBusy = false;
                }
            }, canExecute ?? (() => !IsBusy));
        }

        // Version pour les commandes synchrones
        protected ICommand CreateCommand(Action execute, Func<bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new Command(() =>
            {
                if (IsBusy) return;

                try
                {
                    IsBusy = true;
                    execute();
                }
                catch (Exception ex)
                {
                    // Pour les actions synchrones, on ne peut pas await
                    Task.Run(async () => await HandleError(ex));
                }
                finally
                {
                    IsBusy = false;
                }
            }, canExecute ?? (() => !IsBusy));
        }

        // Version générique pour les commandes asynchrones avec paramètre
        protected ICommand CreateCommand<T>(Func<T, Task> execute, Func<T, bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new Command<T>(async (param) =>
            {
                if (IsBusy) return;

                try
                {
                    IsBusy = true;
                    await execute(param);
                }
                catch (Exception ex)
                {
                    await HandleError(ex);
                }
                finally
                {
                    IsBusy = false;
                }
            }, canExecute ?? ((_) => !IsBusy));
        }

        // Version générique pour les commandes synchrones avec paramètre
        protected ICommand CreateCommand<T>(Action<T> execute, Func<T, bool> canExecute = null)
        {
            if (execute == null)
                throw new ArgumentNullException(nameof(execute));

            return new Command<T>((param) =>
            {
                if (IsBusy) return;

                try
                {
                    IsBusy = true;
                    execute(param);
                }
                catch (Exception ex)
                {
                    Task.Run(async () => await HandleError(ex));
                }
                finally
                {
                    IsBusy = false;
                }
            }, canExecute ?? ((_) => !IsBusy));
        }

        protected virtual void OnDisappearing()
        {
            // Méthode à surcharger pour le nettoyage
            // ou la sauvegarde d'état si nécessaire
        }
    }
}