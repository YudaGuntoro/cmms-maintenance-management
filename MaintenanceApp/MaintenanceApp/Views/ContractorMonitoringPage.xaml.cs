using MaintenanceApp.ViewsModel;

namespace MaintenanceApp.Views
{
    public partial class ContractorMonitoringPage : ContentPage
    {
        private readonly ContractorMonitoringViewModel _viewModel = new();

        public ContractorMonitoringPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }

        private async void Filter_Clicked(object sender, EventArgs e)
        {
            await _viewModel.LoadAsync();
        }

        private async void RefreshView_Refreshing(object sender, EventArgs e)
        {
            await _viewModel.LoadAsync();
        }
    }
}
