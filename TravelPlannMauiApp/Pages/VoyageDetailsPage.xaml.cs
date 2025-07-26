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
                            Debug.WriteLine($"VoyageId défini: {_viewModel.VoyageId}");
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

    public class VoyageDetailsDTO
    {
        public int VoyageID { get; set; }
        public string NomVoyage { get; set; }
        public string Description { get; set; }
        public DateTime DateDebut { get; set; }
        public DateTime DateFin { get; set; }
        public List<Activite> Activites { get; set; } = new();
        public List<Hebergement> Hebergements { get; set; } = new();
    }
}