using MaintenanceApp.ViewsModel;

namespace MaintenanceApp.Views
{
    public partial class TechnicianListPage : ContentPage
    {
        private readonly TechnicianListViewModel _viewModel = new();

        public TechnicianListPage()
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
