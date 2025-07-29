using TravelPlannMauiApp.ViewModels;

namespace TravelPlannMauiApp.Pages
{
    public partial class AddVoyagePage : ContentPage
    {
        public AddVoyagePage(AddVoyageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
            
            if (viewModel == null)
            {
                throw new ArgumentNullException(nameof(viewModel), "Le ViewModel ne peut pas être nul");
            }
        }
    }
}