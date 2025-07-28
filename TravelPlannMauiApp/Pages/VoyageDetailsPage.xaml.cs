using TravelPlannMauiApp.ViewModels;
using System.Text.Json;
using BU.Services;
using Microsoft.Maui.Controls;
using System.Collections.ObjectModel;
using Common.Helpers;
using DAL.DB;
using System.Diagnostics;

namespace TravelPlannMauiApp.Pages
{
    [QueryProperty(nameof(SerializedViewModel), "ViewModel")]
    public partial class VoyageDetailsPage : ContentPage
    {
        private readonly VoyageDetailsViewModel _viewModel;

        public VoyageDetailsPage(VoyageDetailsViewModel viewModel)
        {
            InitializeComponent();
            Resources.Add("CountToHeightConverter", new CountToHeightConverter());
            
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }
        
        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            // Si les données n'ont pas été chargées via SerializedViewModel,
            // charger depuis la base de données
            if (_viewModel.VoyageId > 0 && string.IsNullOrEmpty(_viewModel.NomVoyage))
            {
                await _viewModel.LoadVoyageDetails();
            }
        }

        public string SerializedViewModel
        {
            set
            {
                Debug.WriteLine($"Reception des données sérialisées: {value}");

                if (!string.IsNullOrEmpty(value))
                {
                    try
                    {
                        var dto = JsonSerializer.Deserialize<VoyageDetailsDTO>(value);
                        Debug.WriteLine($"DTO désérialisé - ID: {dto.VoyageID}, Nom: {dto.NomVoyage}");

                        if (_viewModel != null)
                        {
                            // Utiliser Dispatcher pour s'assurer que les modifications UI se font sur le bon thread
                            Dispatcher.Dispatch(() =>
                            {
                                _viewModel.VoyageId = dto.VoyageID;
                                
                                // Pré-charger les données depuis le DTO
                                _viewModel.NomVoyage = dto.NomVoyage;
                                _viewModel.Description = dto.Description;
                                _viewModel.DateDebut = DateOnly.FromDateTime(dto.DateDebut);
                                _viewModel.DateFin = DateOnly.FromDateTime(dto.DateFin);
                                _viewModel.EstComplete = dto.EstComplete;
                                _viewModel.EstArchive = dto.EstArchive;
                                _viewModel.UtilisateurId = dto.UtilisateurId;
                                
                                // Nettoyer les collections existantes
                                _viewModel.Activites.Clear();
                                _viewModel.Hebergements.Clear();
                                
                                // Convertir les DTOs en entités pour l'affichage
                                if (dto.Activites != null)
                                {
                                    foreach (var activiteDto in dto.Activites)
                                    {
                                        var activite = new Activite
                                        {
                                            ActiviteId = activiteDto.ActiviteId,
                                            Nom = activiteDto.Nom,
                                            Description = activiteDto.Description,
                                            Localisation = activiteDto.Localisation
                                        };
                                        _viewModel.Activites.Add(activite);
                                    }
                                }
                                
                                if (dto.Hebergements != null)
                                {
                                    foreach (var hebergementDto in dto.Hebergements)
                                    {
                                        var hebergement = new Hebergement
                                        {
                                            HebergementId = hebergementDto.HebergementId,
                                            Nom = hebergementDto.Nom,
                                            TypeHebergement = hebergementDto.TypeHebergement,
                                            Cout = hebergementDto.Cout,
                                            DateDebut = hebergementDto.DateDebut,
                                            DateFin = hebergementDto.DateFin,
                                            Adresse = hebergementDto.Adresse
                                        };
                                        _viewModel.Hebergements.Add(hebergement);
                                    }
                                }
                                
                                Debug.WriteLine($"VoyageId défini: {_viewModel.VoyageId}");
                                Debug.WriteLine($"Activités chargées: {_viewModel.Activites.Count}");
                                Debug.WriteLine($"Hébergements chargés: {_viewModel.Hebergements.Count}");
                            });
                        }
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine($"ERREUR de désérialisation: {ex}");
                        Debug.WriteLine($"Données reçues: {value}");
                        
                        // Utiliser Dispatcher pour les opérations UI
                        Dispatcher.Dispatch(async () =>
                        {
                            await Shell.Current.DisplayAlert("Erreur",
                                $"Erreur de désérialisation:\n{ex.Message}", "OK");
                        });
                    }
                }
                else
                {
                    Debug.WriteLine("SerializedViewModel reçu vide ou null");
                }
            }
        }
    }

    // DTOs mis à jour pour correspondre à ceux du ViewModel
    public class VoyageDetailsDTO
    {
        public int VoyageID { get; set; }
        public string NomVoyage { get; set; }
        public string Description { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public bool EstComplete { get; set; }
        public bool EstArchive { get; set; }
        public int UtilisateurId { get; set; }
        public List<ActiviteDTO> Activites { get; set; } = new();
        public List<HebergementDTO> Hebergements { get; set; } = new();
    }
    
    public class ActiviteDTO
    {
        public int ActiviteId { get; set; }
        public string Nom { get; set; }
        public string Description { get; set; }
        public string Localisation { get; set; }
    }
    
    public class HebergementDTO
    {
        public int HebergementId { get; set; }
        public string Nom { get; set; }
        public string TypeHebergement { get; set; }
        public decimal? Cout { get; set; }
        public DateOnly? DateDebut { get; set; }
        public DateOnly? DateFin { get; set; }
        public string Adresse { get; set; }
    }
}