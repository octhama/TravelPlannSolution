
namespace TravelPlannMauiApp.Pages
{
    using TravelPlannMauiApp.ViewModels;
    using Microsoft.Extensions.DependencyInjection;

    public partial class SettingsPage : ContentPage
    {
        private readonly SettingsViewModel _viewModel;

        public SettingsPage(SettingsViewModel viewModel)
        {
            InitializeComponent();
            _viewModel = viewModel;
            BindingContext = _viewModel;
        }

        public SettingsPage()
        {
            throw new NotImplementedException();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadSettingsCommand.Execute(null);
        }
    }
}