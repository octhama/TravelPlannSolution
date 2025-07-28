using BU.Services;
using DAL.DB;
using System.Collections.ObjectModel;
using System.Text.Json;
using System.Windows.Input;
using TravelPlannMauiApp.Pages;
using System.Diagnostics;
using System.ComponentModel;

namespace TravelPlannMauiApp.ViewModels
{
    // VoyageItemViewModel corrigé avec accès public aux méthodes de notification
    public class VoyageItemViewModel : INotifyPropertyChanged
    {
        private Voyage _voyage;
        private bool _estComplete;
        private bool _estArchive;

        public VoyageItemViewModel(Voyage voyage)
        {
            _voyage = voyage ?? throw new ArgumentNullException(nameof(voyage));
            _estComplete = voyage.EstComplete;
            _estArchive = voyage.EstArchive;
        }

        public Voyage Voyage => _voyage;

        public int VoyageId => _voyage.VoyageId;
        public string NomVoyage => _voyage.NomVoyage;
        public string Description => _voyage.Description;
        public DateOnly DateDebut => _voyage.DateDebut;
        public DateOnly DateFin => _voyage.DateFin;
        public int UtilisateurId => _voyage.UtilisateurId;

        public bool EstComplete
        {
            get => _estComplete;
            private set
            {
                if (_estComplete != value)
                {
                    Debug.WriteLine($"EstComplete changé pour {NomVoyage}: {_estComplete} -> {value}");
                    _estComplete = value;
                    _voyage.EstComplete = value;
                    OnPropertyChanged();

                    // Déclencher les mises à jour des propriétés liées
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnPropertyChanged(nameof(EstArchive));
                    });
                }
            }
        }

        public bool EstArchive
        {
            get => _estArchive;
            private set
            {
                if (_estArchive != value)
                {
                    Debug.WriteLine($"EstArchive changé pour {NomVoyage}: {_estArchive} -> {value}");
                    _estArchive = value;
                    _voyage.EstArchive = value;
                    OnPropertyChanged();

                    // Déclencher les mises à jour des propriétés liées
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        OnPropertyChanged(nameof(EstComplete));
                    });
                }
            }
        }

        // MÉTHODE PUBLIQUE pour forcer une mise à jour complète
        public void ForceUpdate()
        {
            try
            {
                Debug.WriteLine($"ForceUpdate pour voyage: {NomVoyage} - Complete: {EstComplete}, Archive: {EstArchive}");
                
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    // Déclencher une mise à jour de toutes les propriétés
                    OnPropertyChanged(string.Empty);
                    
                    // Déclencher spécifiquement les propriétés d'état
                    OnPropertyChanged(nameof(EstComplete));
                    OnPropertyChanged(nameof(EstArchive));
                    OnPropertyChanged(nameof(NomVoyage));
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(DateDebut));
                    OnPropertyChanged(nameof(DateFin));
                });
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur ForceUpdate: {ex}");
            }
        }

        // MÉTHODE PUBLIQUE pour mettre à jour depuis un voyage modifié
        public void UpdateFromVoyage(Voyage updatedVoyage)
        {
            if (updatedVoyage == null) return;
            
            Debug.WriteLine($"UpdateFromVoyage pour {NomVoyage}: Complete {_estComplete}->{updatedVoyage.EstComplete}, Archive {_estArchive}->{updatedVoyage.EstArchive}");
            
            // Vérifier s'il y a vraiment des changements
            var oldComplete = _estComplete;
            var oldArchive = _estArchive;
            
            // Mettre à jour les propriétés directement (sans passer par les setters pour éviter les boucles)
            _estComplete = updatedVoyage.EstComplete;
            _estArchive = updatedVoyage.EstArchive;
            _voyage.EstComplete = updatedVoyage.EstComplete;
            _voyage.EstArchive = updatedVoyage.EstArchive;
            
            // Mettre à jour les autres propriétés si nécessaire
            _voyage.NomVoyage = updatedVoyage.NomVoyage;
            _voyage.Description = updatedVoyage.Description;
            _voyage.DateDebut = updatedVoyage.DateDebut;
            _voyage.DateFin = updatedVoyage.DateFin;
            
            // Déclencher les notifications de changement sur le thread principal
            MainThread.BeginInvokeOnMainThread(() =>
            {
                try
                {
                    // Notifier TOUS les changements possibles
                    OnPropertyChanged(nameof(EstComplete));
                    OnPropertyChanged(nameof(EstArchive));
                    OnPropertyChanged(nameof(NomVoyage));
                    OnPropertyChanged(nameof(Description));
                    OnPropertyChanged(nameof(DateDebut));
                    OnPropertyChanged(nameof(DateFin));
                    
                    // Si des changements significatifs ont eu lieu, forcer une mise à jour complète
                    if (oldComplete != _estComplete || oldArchive != _estArchive)
                    {
                        OnPropertyChanged(string.Empty); // Toutes les propriétés
                        Debug.WriteLine($"Changements détectés et notifiés pour {NomVoyage}");
                    }
                }
                catch (Exception ex)
                {
                    Debug.WriteLine($"Erreur lors de la notification des changements: {ex}");
                }
            });
        }

        public event PropertyChangedEventHandler PropertyChanged;

        // MÉTHODE PUBLIQUE pour les notifications de propriétés
        public void OnPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = null)
        {
            try
            {
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
                Debug.WriteLine($"PropertyChanged notifié pour {propertyName} sur voyage {NomVoyage}");
            }
            catch (Exception ex)
            {
                Debug.WriteLine($"Erreur OnPropertyChanged: {ex}");
            }
        }
    }
}