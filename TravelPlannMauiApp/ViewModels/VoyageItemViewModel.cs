using DAL.DB;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace TravelPlannMauiApp.ViewModels
{
    public class VoyageItemViewModel : INotifyPropertyChanged
    {
        private Voyage _voyage;

        public VoyageItemViewModel(Voyage voyage)
        {
            _voyage = voyage ?? throw new ArgumentNullException(nameof(voyage));
        }

        public Voyage Voyage => _voyage;

        public int VoyageId => _voyage.VoyageId;
        
        public string NomVoyage => _voyage.NomVoyage ?? "";
        
        public string Description => _voyage.Description ?? "";
        
        public DateOnly DateDebut => _voyage.DateDebut;
        
        public DateOnly DateFin => _voyage.DateFin;
        
        public bool EstComplete => _voyage.EstComplete;
        
        public bool EstArchive => _voyage.EstArchive;
        
        public int UtilisateurId => _voyage.UtilisateurId;

        // NOUVEAU : Méthode pour mettre à jour le voyage et notifier les changements
        public void UpdateFromVoyage(Voyage nouveauVoyage)
        {
            if (nouveauVoyage == null) return;

            var ancienEstComplete = _voyage.EstComplete;
            var ancienEstArchive = _voyage.EstArchive;
            var ancienNom = _voyage.NomVoyage;
            var ancienneDescription = _voyage.Description;

            _voyage = nouveauVoyage;

            // Notifier tous les changements potentiels
            OnPropertyChanged(nameof(NomVoyage));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(DateDebut));
            OnPropertyChanged(nameof(DateFin));
            OnPropertyChanged(nameof(EstComplete));
            OnPropertyChanged(nameof(EstArchive));

            System.Diagnostics.Debug.WriteLine($"VoyageItemViewModel mis à jour: {NomVoyage} - Complete: {EstComplete}, Archive: {EstArchive}");
        }

        // NOUVEAU : Méthode pour forcer la mise à jour de l'affichage
        public void ForceUpdate()
        {
            OnPropertyChanged(nameof(NomVoyage));
            OnPropertyChanged(nameof(Description));
            OnPropertyChanged(nameof(DateDebut));
            OnPropertyChanged(nameof(DateFin));
            OnPropertyChanged(nameof(EstComplete));
            OnPropertyChanged(nameof(EstArchive));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}