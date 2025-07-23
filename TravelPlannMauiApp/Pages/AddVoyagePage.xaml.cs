using TravelPlannMauiApp.ViewModels;

namespace TravelPlannMauiApp.Pages
{
    public partial class AddVoyagePage : ContentPage
    {
        public AddVoyagePage(AddVoyageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            
            viewModel.VoyageAdded += async (sender, voyage) => 
            {
                //await Shell.Current.GoToAsync("..");
            };
        }
    }
}