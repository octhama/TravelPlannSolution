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
            
            // Attendre que les données soient chargées
            if (_viewModel.VoyageId > 0)
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
                            _viewModel.VoyageId = dto.VoyageID;
                            
                            // Pré-charger les données depuis le DTO
                            _viewModel.NomVoyage = dto.NomVoyage;
                            _viewModel.Description = dto.Description;
                            _viewModel.DateDebut = DateOnly.FromDateTime(dto.DateDebut);
                            _viewModel.DateFin = DateOnly.FromDateTime(dto.DateFin);
                            _viewModel.EstComplete = dto.EstComplete;
                            _viewModel.EstArchive = dto.EstArchive;
                            _viewModel.UtilisateurId = dto.UtilisateurId;
                            _viewModel.EstComplete = dto.EstComplete;
                            _viewModel.EstArchive = dto.EstArchive;
                            
                            // Convertir les DTOs en entités pour l'affichage
                            var activites = dto.Activites?.Select(a => new Activite
                            {
                                ActiviteId = a.ActiviteId,
                                Nom = a.Nom,
                                Description = a.Description,
                                Localisation = a.Localisation
                            }).ToList() ?? new List<Activite>();
                            
                            var hebergements = dto.Hebergements?.Select(h => new Hebergement
                            {
                                HebergementId = h.HebergementId,
                                Nom = h.Nom,
                                TypeHebergement = h.TypeHebergement,
                                Cout = h.Cout,
                                DateDebut = h.DateDebut,
                                DateFin = h.DateFin,
                                Adresse = h.Adresse
                            }).ToList() ?? new List<Hebergement>();
                            
                            // Mettre à jour les collections
                            _viewModel.Activites.Clear();
                            foreach (var activite in activites)
                            {
                                _viewModel.Activites.Add(activite);
                            }
                            
                            _viewModel.Hebergements.Clear();
                            foreach (var hebergement in hebergements)
                            {
                                _viewModel.Hebergements.Add(hebergement);
                            }
                            
                            Debug.WriteLine($"VoyageId défini: {_viewModel.VoyageId}");
                            Debug.WriteLine($"Activités chargées: {_viewModel.Activites.Count}");
                            Debug.WriteLine($"Hébergements chargés: {_viewModel.Hebergements.Count}");
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