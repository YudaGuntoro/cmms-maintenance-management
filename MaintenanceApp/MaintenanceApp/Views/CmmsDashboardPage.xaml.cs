using MaintenanceApp.ViewsModel;

namespace MaintenanceApp.Views
{
    public partial class CmmsDashboardPage : ContentPage
    {
        private readonly CmmsDashboardViewModel _viewModel = new();

        public CmmsDashboardPage()
        {
            InitializeComponent();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadAsync();
        }

        private async void RefreshView_Refreshing(object sender, EventArgs e)
        {
            await _viewModel.LoadAsync();
        }
    }
}
